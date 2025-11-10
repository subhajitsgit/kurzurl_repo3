using KurzUrl.BusinessLayer.Interface;
using KurzUrl.Repository.Interface;
using KurzUrl.Repository.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using KurzUrl.Repository.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;


namespace KurzUrl.Controllers.Admin_Interface
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsereLinksController : ControllerBase
     {
        public IKurzUrlRepo _kurzUrlRepo { get; set; }
        public IHttpContextAccessor _HttpContextAccessor { get; set; }
        private readonly ShortUrlContext _context;
        public IKurzUrl _kurzUrlBL { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;

        public UsereLinksController(
            IKurzUrlRepo kurzUrlRepo,
            IHttpContextAccessor httpContextAccessor,
            ShortUrlContext context,
            IKurzUrl kurzUrl,
            UserManager<ApplicationUser> userManager)
        {
            _kurzUrlRepo = kurzUrlRepo;
            _HttpContextAccessor = httpContextAccessor;
            _context = context;
            _kurzUrlBL = kurzUrl;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("user-links")]
        public async Task<List<TblUrlDetail>> GetLinksCreatedByUserId(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "";
            var results = await _kurzUrlRepo.GetUrlsCreatedBy(userId, cancellationToken);
            return results;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create-link")]
        public async Task<ActionResult> CreateLink([FromBody] CreateLinkRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest("Invalid request");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var mainUrl = request.MainUrl;
            var title = request.Title;

            var planId = await _kurzUrlRepo.GetLatestUserPlanAsync(userId, cancellationToken);
            var pricingLimitDto = await _kurzUrlRepo.GetPricingLimitAsync(planId, 1, cancellationToken);

            if (pricingLimitDto == null)
            {
                return BadRequest(new { message = "Pricing limit not found for this given plan." });
            }
            int pricingLimit = pricingLimitDto.Limit;
            int createdLinks = await this._kurzUrlRepo.GetMonthlyCreatedLinkCount(userId, cancellationToken);
            if (createdLinks >= pricingLimit)
            {
                return BadRequest(new
                {
                    isPlanLimitReached = true,
                    message = $"{pricingLimitDto.PlanName} reached",
                    planName = pricingLimitDto.PlanName,
                    planType = pricingLimitDto.PlanType,
                    planLimit = pricingLimitDto.Limit,
                    currentLinks = createdLinks,
                    upgradeTo = pricingLimitDto.UpgradeToPlanName
                });
            }

            string shortUrl = _kurzUrlBL.GenerateKurzUrl();

            var entity = new TblUrlDetail
            {
                Title = title,
                MainUrl = mainUrl,
                ShortUrl = shortUrl,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                IsActive = true
            };

            int newId = _kurzUrlRepo.SaveKurzUrl(entity);

            return Ok(new
            {
                id = newId,
                mainUrl = entity.MainUrl,
                shortUrl = entity.ShortUrl,
                createdOn = entity.CreatedOn,
                message = "short URL created successfully!"
            });
        }

        [HttpGet("check-month-limit-sp/{userId:int}")]
        public async Task<ActionResult<int>> CheckMonthLimitSP(int userId)
        {
            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_CheckMonthLimit";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId));

            var count = (int)await cmd.ExecuteScalarAsync();

            return Ok(count);
        }
    }
}
