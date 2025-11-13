using KurzUrl.BusinessLayer.Interface;
using KurzUrl.Repository.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KurzUrl.Controllers.Admin_Interface
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRController: ControllerBase
    {
        private readonly IQRService _qrService;

        public QRController(IQRService qrService)
        {
            _qrService = qrService;
        }

        [HttpGet("generate")]
        public ActionResult Generate([FromQuery] string url)
        {
            try
            {
                string qrBase64 = _qrService.GenerateQR(url);
                return Ok(new { qrBase64 });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating QR", detail = ex.Message });
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get")]
        public async Task<List<TblQRDetail>> GetQRsCreatedByUserId(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "";
            return await _qrService.GetQRsCreatedByUserIdAsync(userId, cancellationToken);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("save")]
        public async Task<ActionResult> Save([FromBody] SaveQRRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var result = await _qrService.SaveQRAsync(request, userId, cancellationToken);

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
                        currentQRs = result.PlanLimitReached.CurrentCount,
                        upgradeTo = result.PlanLimitReached.UpgradeTo
                    });
                }

                if (result.Message == "User not authenticated")
                    return Unauthorized(new { message = result.Message });

                return BadRequest(new { message = result.Message });
            }

            return Ok(new
            {
                id = result.Id,
                mainUrl = result.MainUrl,
                qrBase64 = result.QRImage,
                createdOn = result.CreatedOn,
                message = result.Message
            });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update")]
        public async Task<ActionResult> Update([FromBody] UpdateQRRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var result = await _qrService.UpdateQRAsync(request, userId, cancellationToken);

            if (!result.Success)
            {
                if (result.Message == "User not authenticated")
                    return Unauthorized(new { message = result.Message });
                
                if (result.Message == "QR code not found")
                    return NotFound(new { message = result.Message });
                
                if (result.Message == "You don't have permission to update this QR code")
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
