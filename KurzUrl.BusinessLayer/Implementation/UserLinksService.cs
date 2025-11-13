using KurzUrl.BusinessLayer.Interface;
using KurzUrl.Repository.DTOs;
using KurzUrl.Repository.Interface;
using KurzUrl.Repository.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KurzUrl.BusinessLayer.Implementation
{
    public class UserLinksService : IUserLinksService
    {
        private readonly IKurzUrlRepo _kurzUrlRepo;
        private readonly IKurzUrl _kurzUrlBL;

        public UserLinksService(IKurzUrlRepo kurzUrlRepo, IKurzUrl kurzUrlBL)
        {
            _kurzUrlRepo = kurzUrlRepo;
            _kurzUrlBL = kurzUrlBL;
        }

        public async Task<List<TblUrlDetail>> GetLinksCreatedByUserIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await _kurzUrlRepo.GetUrlsCreatedBy(userId, cancellationToken);
        }

        public async Task<CreateLinkResult> CreateLinkAsync(CreateLinkRequest request, string userId, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return new CreateLinkResult
                {
                    Success = false,
                    Message = "Invalid request"
                };
            }

            var planId = await _kurzUrlRepo.GetLatestUserPlanAsync(userId, cancellationToken);
            var pricingLimitDto = await _kurzUrlRepo.GetPricingLimitAsync(planId, 1, cancellationToken);

            if (pricingLimitDto == null)
            {
                return new CreateLinkResult
                {
                    Success = false,
                    Message = "Pricing limit not found for this given plan."
                };
            }

            int pricingLimit = pricingLimitDto.Limit;
            int createdLinks = await _kurzUrlRepo.GetMonthlyCreatedLinkCount(userId, cancellationToken);
            
            if (createdLinks >= pricingLimit)
            {
                return new CreateLinkResult
                {
                    Success = false,
                    PlanLimitReached = new PlanLimitReached
                    {
                        IsPlanLimitReached = true,
                        Message = $"{pricingLimitDto.PlanName} reached",
                        PlanName = pricingLimitDto.PlanName,
                        PlanType = pricingLimitDto.PlanType,
                        PlanLimit = pricingLimitDto.Limit,
                        CurrentCount = createdLinks,
                        UpgradeTo = pricingLimitDto.UpgradeToPlanName
                    }
                };
            }

            string shortUrl = _kurzUrlBL.GenerateKurzUrl();

            var entity = new TblUrlDetail
            {
                Title = request.Title,
                MainUrl = request.MainUrl,
                ShortUrl = shortUrl,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            int newId = _kurzUrlRepo.SaveKurzUrl(entity);

            return new CreateLinkResult
            {
                Success = true,
                Id = newId,
                MainUrl = entity.MainUrl,
                ShortUrl = entity.ShortUrl,
                CreatedOn = entity.CreatedOn,
                Message = "short URL created successfully!"
            };
        }

        public async Task<UpdateLinkResult> UpdateLinkAsync(UpdateLinkRequest request, string userId, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return new UpdateLinkResult
                {
                    Success = false,
                    Message = "Invalid request"
                };
            }

            if (string.IsNullOrEmpty(userId))
            {
                return new UpdateLinkResult
                {
                    Success = false,
                    Message = "User not authenticated"
                };
            }

            var existingUrl = await _kurzUrlRepo.GetUrlById(request.Id, cancellationToken);
            if (existingUrl == null)
            {
                return new UpdateLinkResult
                {
                    Success = false,
                    Message = "Link not found"
                };
            }

            if (existingUrl.CreatedBy != userId)
            {
                return new UpdateLinkResult
                {
                    Success = false,
                    Message = "You don't have permission to update this Link"
                };
            }

            var entity = new TblUrlDetail
            {
                Id = request.Id,
                MainUrl = request.MainUrl,
                Title = request.Title,
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = userId
            };

            bool updated = await _kurzUrlRepo.UpdateLinkRequest(entity, cancellationToken);

            if (!updated)
            {
                return new UpdateLinkResult
                {
                    Success = false,
                    Message = "Error updating link"
                };
            }

            return new UpdateLinkResult
            {
                Success = true,
                Id = entity.Id,
                MainUrl = entity.MainUrl,
                Title = entity.Title,
                ModifiedOn = entity.ModifiedOn,
                Message = "Link updated successfully!"
            };
        }
    }
}

