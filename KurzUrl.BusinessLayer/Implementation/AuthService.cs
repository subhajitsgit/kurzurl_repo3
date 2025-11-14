using KurzUrl.BusinessLayer.Interface;
using KurzUrl.Repository.Entities;
using KurzUrl.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KurzUrl.BusinessLayer.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ShortUrlContext _dbContext;
        private const string DefaultRole = "User";
        private const string DefaultPassword = "placeholder#ASD.123";

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ShortUrlContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<ApplicationUser?> FindUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            var normalizedEmail = _userManager.NormalizeEmail(email);
            
            var users = await _dbContext.Users
                .Where(u => u.NormalizedEmail == normalizedEmail && !u.IsDeleted)
                .OrderByDescending(u => u.EmailConfirmed)
                .ThenByDescending(u => u.Id)
                .ToListAsync();

            return users.FirstOrDefault();
        }

        public async Task<GoogleAuthResult> ProcessGoogleAuthenticationAsync(
            IEnumerable<Claim> claims, 
            Func<ApplicationUser, Task<string>> tokenGenerator)
        {
            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return new GoogleAuthResult
                {
                    Success = false,
                    Message = "Email not found in Google account"
                };
            }

            if (lastName == null) lastName = firstName;
            if (firstName == null) firstName = "User";

            var existingUser = await FindUserByEmailAsync(email);

            if (existingUser != null)
            {
                var token = await tokenGenerator(existingUser);
                return new GoogleAuthResult
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    Email = existingUser.Email,
                    Name = existingUser.UserName
                };
            }

            var newUser = new ApplicationUser
            {
                UserName = firstName + lastName,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                AccessFailedCount = 0,
                LockoutEnabled = false,
                TwoFactorEnabled = false
            };

            var createResult = await _userManager.CreateAsync(newUser, DefaultPassword);

            if (!createResult.Succeeded)
            {
                return new GoogleAuthResult
                {
                    Success = false,
                    Message = createResult.Errors.First().Description,
                    Errors = createResult.Errors.ToArray()
                };
            }

            var createdUser = await _userManager.FindByNameAsync(newUser.UserName);

            if (createdUser != null)
            {
                var roles = new[] { DefaultRole };
                await _userManager.AddToRolesAsync(createdUser, roles);

                var token = await tokenGenerator(createdUser);
                return new GoogleAuthResult
                {
                    Success = true,
                    Message = "Registration successful",
                    Token = token,
                    Email = createdUser.Email,
                    Name = createdUser.UserName
                };
            }

            return new GoogleAuthResult
            {
                Success = true,
                Message = "success"
            };
        }

        public async Task<LoginResult> LoginAsync(
            string email, 
            string password, 
            Func<ApplicationUser, Task<string>> tokenGenerator, 
            bool isDevelopment,
            Func<ApplicationUser, string, Task<bool>> passwordChecker)
        {
            if (!isDevelopment)
            {
                if (email != "admin@gmail.com" || password != "Admin123*")
                {
                    return new LoginResult
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                var adminUser = new ApplicationUser
                {
                    Email = email,
                    Id = "27d131eb-0da6-4d8e-a70d-8ba9d8536810"
                };

                var adminToken = await tokenGenerator(adminUser);
                return new LoginResult
                {
                    Success = true,
                    Message = "Successfully Authorized",
                    Token = adminToken,
                    Email = email
                };
            }

            var user = await FindUserByEmailAsync(email);
            
            if (user == null)
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            var isPasswordValid = await passwordChecker(user, password);
            
            if (!isPasswordValid)
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            var userToken = await tokenGenerator(user);
            
            return new LoginResult
            {
                Success = true,
                Message = "Successfully Authorized",
                Token = userToken,
                Email = user.Email,
                Name = user.UserName
            };
        }

        public async Task<RegisterResult> RegisterAsync(
            string userName, 
            string email, 
            string password, 
            string firstName, 
            string lastName, 
            Func<ApplicationUser, Task<string>>? tokenGenerator = null)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = false,
                FirstName = firstName,
                LastName = lastName,
                AccessFailedCount = 0,
                LockoutEnabled = false,
                TwoFactorEnabled = false
            };

            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                return new RegisterResult
                {
                    Success = false,
                    Message = createResult.Errors.First().Description,
                    Errors = createResult.Errors.ToArray()
                };
            }

            var createdUser = await _userManager.FindByNameAsync(userName);
            
            if (createdUser == null)
            {
                return new RegisterResult
                {
                    Success = false,
                    Message = "User created but could not be found"
                };
            }

            var roles = new[] { DefaultRole };
            await _userManager.AddToRolesAsync(createdUser, roles);

            string? token = null;
            if (tokenGenerator != null)
            {
                token = await tokenGenerator(createdUser);
            }

            return new RegisterResult
            {
                Success = true,
                Message = "User registered successfully",
                Token = token,
                UserId = createdUser.Id,
                UserName = createdUser.UserName
            };
        }
    }
}

