using KurzUrl.Data.Dto;
using KurzUrl.Repository.Entities;
using KurzUrl.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;

namespace KurzUrl.Controllers.UserI_Interface
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJWTService _jWTService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Microsoft.Extensions.Hosting.IHostingEnvironment _hostingEnvironment;
        public AuthController(IJWTService jWTService, UserManager<ApplicationUser> userManager,
            Microsoft.Extensions.Hosting.IHostingEnvironment configuration)
        {
            _jWTService = jWTService;
            _userManager = userManager;
            _hostingEnvironment = configuration;
        }



        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequest request)
        {
            if (_hostingEnvironment.IsDevelopment())
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    return Unauthorized("Invalid username or password");
                }

                // Authentication successful, generate JWT
                var token = await _jWTService.GenerateToken(user);
                return Ok(new { Token = token, Reslt = "Successfully Authorized" });
            }
            else
            {
                if (request.Email != "admin@gmail.com" && request.Password != "Admin123*")
                {
                    return Unauthorized("Invalid username or password");
                }
                ApplicationUser applicationUser = new ApplicationUser();
                applicationUser.Email = request.Email;
                applicationUser.Id = "27d131eb-0da6-4d8e-a70d-8ba9d8536810";
                // Authentication successful, generate JWT
                var token = await _jWTService.GenerateToken(applicationUser);
                return Ok(new { Token = token, Reslt = "Successfully Authorized" });
            }

        }

        [HttpGet("google/login")]
        public async Task<IActionResult> GoogleLogin()
        {
            //var redirectUrl = Url.Action("GoogleSuccess", "Auth", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = "/api/auth/google/login-success" };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google/login-success")]
        public async Task<IActionResult> GoogleLoginSuccess()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            string json;
            if (!result.Succeeded)
            {
                Console.WriteLine(result?.Failure?.ToString());
                json = JsonSerializer.Serialize(new { success = false, message = "Google Authentication Failed", token = "" });

                return Content($"<script>window.opener.postMessage({json},'*'); window.close();</script>", "text/html");
            }


            var claims = result.Principal.Claims;


            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (lastName == null) lastName = firstName;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, "placeholder#ASD.123"))
            {
                json = JsonSerializer.Serialize(new { success = false, message = "Invalid username or password",  token = "" });

                return Content($"<script>window.opener.postMessage({json},'*'); window.close();</script>", "text/html");
            }

            // Authentication successful, generate JWT
            var token = await _jWTService.GenerateToken(user);

            json = JsonSerializer.Serialize(new { success = true, message = "successful", token = token, email = user.Email, name = user.UserName });


            return Content($"<script>window.opener.postMessage({json},'*'); window.close();</script>", "text/html");

        }

        [HttpGet("google/register")]
        public async Task<IActionResult> GoogleRegister()
        {
            //var redirectUrl = Url.Action("GoogleSuccess", "Auth", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = "/api/auth/google/success" };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google/success")]
        public async Task<IActionResult> GoogleSuccess()
        {
            List<string> rtn = new List<string>()
             {
                 "User"
             };
            string json;
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                json = JsonSerializer.Serialize(new { success = false, message = "google auth failed" });
                Console.WriteLine(result?.Failure?.ToString());
                return Content($"<script>window.opener.postMessage({json},'*'); window.close();</script>", "text/html");

            }


            var claims = result.Principal.Claims;


            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (lastName == null) lastName = firstName;
            var user = new ApplicationUser
            {
                UserName = firstName+lastName,
                Email = email,
                EmailConfirmed = false,
                FirstName = firstName,
                LastName = lastName,
                AccessFailedCount = 0,
                LockoutEnabled = false,
                TwoFactorEnabled = false
            };

            var isRegistered = await _userManager.FindByEmailAsync(user.Email);

            if (isRegistered != null)
            {
                json = JsonSerializer.Serialize(new { success = false, message = "Already registered email"});
                return Content($"<script>window.opener.postMessage({json},'*'); window.close();</script>", "text/html");

            }

            var res = await _userManager.CreateAsync(user, "placeholder#ASD.123");

            if (!res.Succeeded)
            {
                json = JsonSerializer.Serialize(new { success = false, message = res.Errors.First().Description, errors = res.Errors });
                return Content($"<script>window.opener.postMessage({json},'*'); window.close();</script>", "text/html");

            }

            var userRtn = await _userManager.FindByNameAsync(user.UserName);

            var roleRtn = await _userManager.AddToRolesAsync(userRtn, rtn);


            json = JsonSerializer.Serialize(new { success = true, message = "success"});

            return Content($"<script>window.opener.postMessage({json},'*'); window.close();</script>", "text/html");
        }


        [HttpPost("register")]
       // [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            List<string> rtn = new List<string>()
             {
                 "User"
             };
            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                EmailConfirmed = false,
                FirstName = dto.firstName,
                LastName = dto.lastName,
                AccessFailedCount = 0,
                LockoutEnabled = false,
                TwoFactorEnabled = false
            };


            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded && result.Errors.Count() > 0)
            {
                return BadRequest(result.Errors.First().Description);
            }

            var userRtn = await _userManager.FindByNameAsync(user.UserName);

            var roleRtn = await _userManager.AddToRolesAsync(userRtn, rtn);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "User registered successfully", user.Id, user.UserName });
        }
    }
    public record RegisterUserDto(string UserName, string Email, string Password, string firstName, string lastName);
}
