using KurzUrl.BusinessLayer.Interface;
using KurzUrl.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KurzUrl.Controllers.Admin_Interface
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsereLinksController : ControllerBase
    {
        private readonly IUserLinksService _userLinksService;

        public UsereLinksController(IUserLinksService userLinksService)
        {
            _userLinksService = userLinksService;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("user-links")]
        public async Task<List<TblUrlDetail>> GetLinksCreatedByUserId(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "";
            return await _userLinksService.GetLinksCreatedByUserIdAsync(userId, cancellationToken);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create-link")]
        public async Task<ActionResult> CreateLink([FromBody] CreateLinkRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var result = await _userLinksService.CreateLinkAsync(request, userId, cancellationToken);

            if (!result.Success)
            {
                if (result.PlanLimitReached != null)
                {
                    return BadRequest(new
                    {
                        isPlanLimitReached = result.PlanLimitReached.IsPlanLimitReached,
                        message = result.PlanLimitReached.Message,
                        planName = result.PlanLimitReached.PlanName,
                        planType = result.PlanLimitReached.PlanType,
                        planLimit = result.PlanLimitReached.PlanLimit,
                        currentLinks = result.PlanLimitReached.CurrentCount,
                        upgradeTo = result.PlanLimitReached.UpgradeTo
                    });
                }

                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                id = result.Id,
                mainUrl = result.MainUrl,
                shortUrl = result.ShortUrl,
                createdOn = result.CreatedOn,
                message = result.Message
            });
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update")]
        public async Task<ActionResult> Update([FromBody] UpdateLinkRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var result = await _userLinksService.UpdateLinkAsync(request, userId, cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "User not authenticated")
                    return Unauthorized(new { message = result.Message });
                
                if (result.Message == "Link not found")
                    return NotFound(new { message = result.Message });
                
                if (result.Message == "You don't have permission to update this Link")
                    return Forbid(result.Message);
                
                return StatusCode(500, new { message = result.Message });
            }

            return Ok(new
            {
                id = result.Id,
                mainUrl = result.MainUrl,
                title = result.Title,
                modifiedOn = result.ModifiedOn,
                message = result.Message
            });
        }
    }
}
