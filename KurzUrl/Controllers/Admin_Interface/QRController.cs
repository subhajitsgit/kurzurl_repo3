using KurzUrl.BusinessLayer.Interface;
using KurzUrl.Repository.Interface;
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
        public IKurzUrlRepo _kurzUrlRepo {  get; set; }
        private readonly IQRGenerator _qrGenerator;

        public QRController(
            IQRGenerator qrGenerator,
            IKurzUrlRepo kurzUrlRepo
            )
        {
            _qrGenerator = qrGenerator;
            _kurzUrlRepo = kurzUrlRepo;
        }

        [HttpGet("generate")]
        public ActionResult Generate([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace((url)))
                return BadRequest(new { message = "URL cannot be empty" });

            try
            {
                string qrBase64 = _qrGenerator.GenerateQR(url);
                return Ok(new { qrBase64 });
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
            var results = await _kurzUrlRepo.GetQRsCreatedBy(userId, cancellationToken);
            return results;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("save")]
        public async Task<ActionResult> Save([FromBody] SaveQRRequest request, CancellationToken cancellationToken)
        {
            if (Request == null)
                return BadRequest("Invalid request");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var mainUrl = request.MainUrl;
            var title = request.Title;
            var qrBase64 = request.QRBase64;

            var planId = await _kurzUrlRepo.GetLatestUserPlanAsync(userId, cancellationToken);
            var pricingLimitDto = await _kurzUrlRepo.GetPricingLimitAsync(planId, 2, cancellationToken);
            
            if (pricingLimitDto == null)
            {
                return BadRequest(new { message = "Pricing limit not found for this given plan." });
            }
            int pricingLimit = pricingLimitDto.Limit;
            int createdQRs = await this._kurzUrlRepo.GetMonthlyCreatedQRCount(userId, cancellationToken);
            if (createdQRs >= pricingLimit)
            {
                return BadRequest(new
                {
                    isPlanLimitReached = true,
                    message = $"{pricingLimitDto.PlanName} reached",
                    planName = pricingLimitDto.PlanName,
                    planType = pricingLimitDto.PlanType,
                    planLimit = pricingLimitDto.Limit,
                    currentQRs = createdQRs,
                    upgradeTo = pricingLimitDto.UpgradeToPlanName
                });
            }

            var entity = new TblQRDetail
            {
                MainUrl = mainUrl,
                Title = title,
                QRImage = Convert.FromBase64String(qrBase64),
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                IsActive = true
            };

            int newId = _kurzUrlRepo.SaveQRRequest(entity);

            return Ok(new
            {
                id = newId,
                mainUrl = entity.MainUrl,
                qrBase64 = entity.QRImage,
                createdOn = entity.CreatedOn,
                message = "QR code created successfully!"
            });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update")]
        public async Task<ActionResult> Update([FromBody] UpdateQRRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            // Verificar que el QR existe y pertenece al usuario
            var existingQR = await _kurzUrlRepo.GetQRById(request.Id, cancellationToken);
            if (existingQR == null)
                return NotFound(new { message = "QR code not found" });

            if (existingQR.CreatedBy != userId)
                return Forbid("You don't have permission to update this QR code");

            // Actualizar el QR
            var entity = new TblQRDetail
            {
                Id = request.Id,
                MainUrl = request.MainUrl,
                Title = request.Title,
                QRImage = Convert.FromBase64String(request.QRBase64),
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = userId
            };

            bool updated = await _kurzUrlRepo.UpdateQRRequest(entity, cancellationToken);
            
            if (!updated)
                return StatusCode(500, new { message = "Error updating QR code" });

            return Ok(new
            {
                id = entity.Id,
                mainUrl = entity.MainUrl,
                title = entity.Title,
                modifiedOn = entity.ModifiedOn,
                message = "QR code updated successfully!"
            });
        }
    }
}
