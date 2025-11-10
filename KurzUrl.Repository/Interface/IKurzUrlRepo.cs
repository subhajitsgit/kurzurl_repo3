using KurzUrl.Repository.DTOs;
using KurzUrl.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Repository.Interface
{
    public interface IKurzUrlRepo
    {
        int SaveKurzUrl(TblUrlDetail tblUrlDetail);
        //TblUrlDetail GetKurzUrlDetail(string shortUrl);
        TblUrlDetail GetKurzUrlDetail(int id);

        Task<string?> GetMainUrlFromShortUrl(string shortUrl);
        Task<List<TblUrlDetail>> GetUrlsCreatedBy(string userId, CancellationToken ct);
        Task<int> GetLatestUserPlanAsync(string userId, CancellationToken ct);
        Task<int> GetMonthlyCreatedLinkCount(string userId, CancellationToken ct);
        Task<int> GetMonthlyCreatedQRCount(string userId, CancellationToken cancellationToken);
        Task<List<TblQRDetail>> GetQRsCreatedBy(string userId, CancellationToken cancellationToken);
        int SaveQRRequest(TblQRDetail tblQRDetail);
        Task<PricingLimitDto?> GetPricingLimitAsync(int planId, int resourceId, CancellationToken cancellationToken);
    }
}
