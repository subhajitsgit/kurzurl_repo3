using KurzUrl.Repository.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KurzUrl.BusinessLayer.Interface
{
    public interface IAuthService
    {
        Task<GoogleAuthResult> ProcessGoogleAuthenticationAsync(IEnumerable<Claim> claims, Func<ApplicationUser, Task<string>> tokenGenerator);
        Task<ApplicationUser?> FindUserByEmailAsync(string email);
        Task<LoginResult> LoginAsync(string email, string password, Func<ApplicationUser, Task<string>> tokenGenerator, bool isDevelopment, Func<ApplicationUser, string, Task<bool>> passwordChecker);
        Task<RegisterResult> RegisterAsync(string userName, string email, string password, string firstName, string lastName, Func<ApplicationUser, Task<string>>? tokenGenerator = null);
    }

    public class GoogleAuthResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public IdentityError[]? Errors { get; set; }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
    }

    public class RegisterResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public IdentityError[]? Errors { get; set; }
    }
}

