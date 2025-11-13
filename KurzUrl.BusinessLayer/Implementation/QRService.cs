using KurzUrl.BusinessLayer.Interface;
using KurzUrl.Repository.Interface;
using KurzUrl.Repository.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KurzUrl.BusinessLayer.Implementation
{
    public class QRService : IQRService
    {
        private readonly IKurzUrlRepo _kurzUrlRepo;
        private readonly IQRGenerator _qrGenerator;

        public QRService(IKurzUrlRepo kurzUrlRepo, IQRGenerator qrGenerator)
        {
            _kurzUrlRepo = kurzUrlRepo;
            _qrGenerator = qrGenerator;
        }

        public string GenerateQR(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be empty", nameof(url));
            }

            try
            {
                return _qrGenerator.GenerateQR(url);
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating QR", ex);
            }
        }

        public async Task<List<TblQRDetail>> GetQRsCreatedByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _kurzUrlRepo.GetQRsCreatedBy(userId, cancellationToken);
        }

        public async Task<SaveQRResult> SaveQRAsync(SaveQRRequest request, string userId, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return new SaveQRResult
                {
                    Success = false,
                    Message = "Invalid request"
                };
            }

            if (string.IsNullOrEmpty(userId))
            {
                return new SaveQRResult
                {
                    Success = false,
                    Message = "User not authenticated"
                };
            }

            var planId = await _kurzUrlRepo.GetLatestUserPlanAsync(userId, cancellationToken);
            var pricingLimitDto = await _kurzUrlRepo.GetPricingLimitAsync(planId, 2, cancellationToken);

            if (pricingLimitDto == null)
            {
                return new SaveQRResult
                {
                    Success = false,
                    Message = "Pricing limit not found for this given plan."
                };
            }

            int pricingLimit = pricingLimitDto.Limit;
            int createdQRs = await _kurzUrlRepo.GetMonthlyCreatedQRCount(userId, cancellationToken);

            if (createdQRs >= pricingLimit)
            {
                return new SaveQRResult
                {
                    Success = false,
                    PlanLimitReached = new PlanLimitReached
                    {
                        IsPlanLimitReached = true,
                        Message = $"{pricingLimitDto.PlanName} reached",
                        PlanName = pricingLimitDto.PlanName,
                        PlanType = pricingLimitDto.PlanType,
                        PlanLimit = pricingLimitDto.Limit,
                        CurrentCount = createdQRs,
                        UpgradeTo = pricingLimitDto.UpgradeToPlanName
                    }
                };
            }

            var entity = new TblQRDetail
            {
                MainUrl = request.MainUrl,
                Title = request.Title,
                QRImage = Convert.FromBase64String(request.QRBase64),
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            int newId = _kurzUrlRepo.SaveQRRequest(entity);

            return new SaveQRResult
            {
                Success = true,
                Id = newId,
                MainUrl = entity.MainUrl,
                QRImage = entity.QRImage,
                CreatedOn = entity.CreatedOn,
                Message = "QR code created successfully!"
            };
        }

        public async Task<UpdateQRResult> UpdateQRAsync(UpdateQRRequest request, string userId, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return new UpdateQRResult
                {
                    Success = false,
                    Message = "Invalid request"
                };
            }

            if (string.IsNullOrEmpty(userId))
            {
                return new UpdateQRResult
                {
                    Success = false,
                    Message = "User not authenticated"
                };
            }

            var existingQR = await _kurzUrlRepo.GetQRById(request.Id, cancellationToken);
            if (existingQR == null)
            {
                return new UpdateQRResult
                {
                    Success = false,
                    Message = "QR code not found"
                };
            }

            if (existingQR.CreatedBy != userId)
            {
                return new UpdateQRResult
                {
                    Success = false,
                    Message = "You don't have permission to update this QR code"
                };
            }

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
            {
                return new UpdateQRResult
                {
                    Success = false,
                    Message = "Error updating QR code"
                };
            }

            return new UpdateQRResult
            {
                Success = true,
                Id = entity.Id,
                MainUrl = entity.MainUrl,
                Title = entity.Title,
                ModifiedOn = entity.ModifiedOn,
                Message = "QR code updated successfully!"
            };
        }
    }
}

