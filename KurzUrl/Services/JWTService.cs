
using IdentityModel;
using KurzUrl.Repository.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Services
{
    public class JWTService : IJWTService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public JWTService(UserManager<ApplicationUser> userManager, IConfiguration config, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _config = config;
            _roleManager = roleManager;
        }

        public async Task<string> GenerateToken(ApplicationUser user)
        {
            // 1. Get roles
            var userRoles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var roleName in userRoles)
            {
                roleClaims.Add(new Claim(JwtClaimTypes.Role, roleName));

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var claimsForRole = await _roleManager.GetClaimsAsync(role);
                    roleClaims.AddRange(claimsForRole);
                }
            }

            var claims = new List<Claim>
     {
         new Claim(JwtRegisteredClaimNames.Sub, user.Id),
         new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
     }
            .Union(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTBearerSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JWTBearerSettings:Issuer"],
                audience: _config["JWTBearerSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ApplicationUser?> GetUserByID(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }
    }
}