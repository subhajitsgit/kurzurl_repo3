
using KurzUrl.Repository.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Services
{
    public interface IJWTService
    {
        Task<string> GenerateToken(ApplicationUser user);
        Task<ApplicationUser?> GetUserByID(string userId);
    }
}
