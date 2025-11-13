using KurzUrl.Repository.DTOs;
using KurzUrl.Repository.Interface;
using KurzUrl.Repository.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Repository.Implementation
{
    public class KurzUrlRepo : IKurzUrlRepo
    {
        private readonly ShortUrlContext _context;
        public KurzUrlRepo(ShortUrlContext shortUrlContext)
        {
            _context = shortUrlContext;
        }
        public int SaveKurzUrl(TblUrlDetail tblUrlDetail)
        {
            //using (var db = new ShortUrlContext())
            //{
            _context.TblUrlDetails.Add(tblUrlDetail);
            _context.SaveChanges();
            return tblUrlDetail.Id;
            //}
        }

        public TblUrlDetail GetKurzUrlDetail(int id)
        {
            //var context = new ShortUrlContext();
            return _context.TblUrlDetails.Where(x => x.Id == id).First();
        }

        public async Task<string?> GetMainUrlFromShortUrl(string shortUrl)
        {
            var rtn = await _context.TblUrlDetails.Where(ss => ss.ShortUrl == shortUrl)
                .FirstOrDefaultAsync();

            return rtn?.MainUrl;
        }

        public async Task<List<TblUrlDetail>> GetUrlsCreatedBy(string userId, CancellationToken ct)
        {
           return await _context.TblUrlDetails
                .Where(x => x.CreatedBy == userId)
                .ToListAsync();
        }

        public async Task<TblUrlDetail?> GetUrlById(int id, CancellationToken cancellationToken)
        {
            return await _context.TblUrlDetails
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> UpdateLinkRequest(TblUrlDetail tblUrlDetail, CancellationToken cancellationToken)
        {
            var existingUrl = await _context.TblUrlDetails
                .Where(x => x.Id == tblUrlDetail.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingUrl == null)
                return false;
            
            existingUrl.MainUrl = tblUrlDetail.MainUrl;
            existingUrl.Title = tblUrlDetail.Title;
            existingUrl.ModifiedOn = tblUrlDetail.ModifiedOn ?? DateTime.UtcNow;
            existingUrl.ModifiedBy = tblUrlDetail.ModifiedBy;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        
        public async Task<int> GetLatestUserPlanAsync(string userId, CancellationToken cancellationToken)
        {
            var latestPlan = await _context.TblUserPricingMappers
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.CreatedOn)
                .Select(u => (int?)u.PlanId)
                .FirstOrDefaultAsync(cancellationToken);

            // Return 0 (Free Plan), if theres no registers
            return latestPlan ?? 0;
        }

        public async Task<PricingLimitDto?> GetPricingLimitAsync(int planId, int resourceId, CancellationToken cancellationToken)
        {
            return await (
                from pricingLimits in _context.TblPricingLimits
                join resources in _context.TblResource on pricingLimits.ResourceId equals resources.ResourceId
                join pricingPlans in _context.TblPricingPlans on pricingLimits.PlanId equals pricingPlans.Id
                join upgradePlan in _context.TblPricingPlans on pricingLimits.UpgradeToPlanId equals upgradePlan.Id into upgradeGroup
                from upgrade in upgradeGroup.DefaultIfEmpty()
                where pricingLimits.PlanId == planId && pricingLimits.ResourceId == resourceId
                select new PricingLimitDto
                {
                    PlanId = pricingLimits.PlanId,
                    ResourceType = resources.ResourceName,
                    Limit = pricingLimits.Limit,
                    PlanType = pricingPlans.PlanType,
                    PlanName = pricingPlans.Name + (!string.IsNullOrEmpty(pricingPlans.PlanType) ? $" ({pricingPlans.PlanType})" : ""),
                    UpgradeToPlanName = upgrade != null
                        ? upgrade.Name + (!string.IsNullOrEmpty(upgrade.PlanType) ? $" ({upgrade.PlanType})" : "")
                        : null
                }
            ).FirstOrDefaultAsync(cancellationToken);
                
        }

        public async Task<int> GetMonthlyCreatedLinkCount(string userId, CancellationToken cancellationToken)
        {
            return await _context.TblUrlDetails
                .Where(u => u.CreatedBy == userId.ToString()
                            && u.CreatedOn.HasValue
                            && u.CreatedOn.Value.Year == DateTime.UtcNow.Year
                            && u.CreatedOn.Value.Month == DateTime.UtcNow.Month)
                .CountAsync(cancellationToken);
        }

        public async Task<int> GetMonthlyCreatedQRCount(string userId, CancellationToken cancellationToken)
        {
            return await _context.TblQRDetails
                .Where(u => u.CreatedBy == userId.ToString()
                            && u.CreatedOn.HasValue
                            && u.CreatedOn.Value.Year == DateTime.UtcNow.Year
                            && u.CreatedOn.Value.Month == DateTime.UtcNow.Month)
                .CountAsync(cancellationToken);
        }

        public async Task<List<TblQRDetail>> GetQRsCreatedBy(string userId, CancellationToken cancellationToken)
        {
            return await _context.TblQRDetails
                .Where(x => x.CreatedBy == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<TblQRDetail?> GetQRById(int id, CancellationToken cancellationToken)
        {
            return await _context.TblQRDetails
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public int SaveQRRequest (TblQRDetail tblQRDetail)
        {
            _context.TblQRDetails.Add(tblQRDetail);
            _context.SaveChanges();
            return tblQRDetail.Id;
        }

        public async Task<bool> UpdateQRRequest(TblQRDetail tblQRDetail, CancellationToken cancellationToken)
        {
            var existingQR = await _context.TblQRDetails
                .Where(x => x.Id == tblQRDetail.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingQR == null)
                return false;

            existingQR.MainUrl = tblQRDetail.MainUrl;
            existingQR.Title = tblQRDetail.Title;
            existingQR.QRImage = tblQRDetail.QRImage;
            existingQR.ModifiedOn = tblQRDetail.ModifiedOn ?? DateTime.UtcNow;
            existingQR.ModifiedBy = tblQRDetail.ModifiedBy;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
