using KurzUrl.BusinessLayer.Interface;
using KurzUrl.Data.Dto;
using KurzUrl.Repository.Entities;
using KurzUrl.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Google;
using System.Linq;
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
        private readonly IAuthService _authService;

        public AuthController(
            IJWTService jWTService, 
            UserManager<ApplicationUser> userManager,
            Microsoft.Extensions.Hosting.IHostingEnvironment configuration,
            IAuthService authService)
        {
            _jWTService = jWTService;
            _userManager = userManager;
            _hostingEnvironment = configuration;
            _authService = authService;
        }



        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequest request)
        {
            var loginResult = await _authService.LoginAsync(
                request.Email,
                request.Password,
                async (user) => await _jWTService.GenerateToken(user),
                _hostingEnvironment.IsDevelopment(),
                async (user, password) => await _userManager.CheckPasswordAsync(user, password));

            if (!loginResult.Success)
            {
                return Unauthorized(loginResult.Message);
            }

            return Ok(new { Token = loginResult.Token, Reslt = loginResult.Message });
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

            if (!result.Succeeded)
            {
                var json = JsonSerializer.Serialize(new { success = false, message = "Google Authentication Failed", token = "" });
                Console.WriteLine(result?.Failure?.ToString());
                return Content($"<script>window.opener.postMessage({json},'*'); window.close();</script>", "text/html");
            }

            var claims = result.Principal.Claims;
            var authResult = await _authService.ProcessGoogleAuthenticationAsync(
                claims, 
                async (user) => await _jWTService.GenerateToken(user));

            var jsonResponse = JsonSerializer.Serialize(new 
            { 
                success = authResult.Success, 
                message = authResult.Message ?? "successful", 
                token = authResult.Token ?? string.Empty, 
                email = authResult.Email ?? string.Empty, 
                name = authResult.Name ?? string.Empty
            });

            return Content($"<script>window.opener.postMessage({jsonResponse},'*'); window.close();</script>", "text/html");
        }

        [HttpGet("google/register")]
        public async Task<IActionResult> GoogleRegister()
        {
            var properties = new AuthenticationProperties { RedirectUri = "/api/auth/google/success" };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google/success")]
        public async Task<IActionResult> GoogleSuccess()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                var json = JsonSerializer.Serialize(new { success = false, message = "google auth failed" });
                Console.WriteLine(result?.Failure?.ToString());
                return Content($"<script>window.opener.postMessage({json},'*'); window.close();</script>", "text/html");
            }

            var claims = result.Principal.Claims;
            var authResult = await _authService.ProcessGoogleAuthenticationAsync(
                claims, 
                async (user) => await _jWTService.GenerateToken(user));

            var jsonResponse = JsonSerializer.Serialize(new 
            { 
                success = authResult.Success, 
                message = authResult.Message, 
                token = authResult.Token ?? string.Empty, 
                email = authResult.Email ?? string.Empty, 
                name = authResult.Name ?? string.Empty,
                errors = authResult.Errors
            });

            return Content($"<script>window.opener.postMessage({jsonResponse},'*'); window.close();</script>", "text/html");
        }


        [HttpPost("register")]
       // [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var registerResult = await _authService.RegisterAsync(
                dto.UserName,
                dto.Email,
                dto.Password,
                dto.firstName,
                dto.lastName);

            if (!registerResult.Success)
            {
                if (registerResult.Errors != null && registerResult.Errors.Length > 0)
                {
                    return BadRequest(registerResult.Errors.First().Description);
                }
                return BadRequest(registerResult.Message);
            }

            return Ok(new { Message = registerResult.Message, Id = registerResult.UserId, UserName = registerResult.UserName });
        }
    }
    public record RegisterUserDto(string UserName, string Email, string Password, string firstName, string lastName);
}
