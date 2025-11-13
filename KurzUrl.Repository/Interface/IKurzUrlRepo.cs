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
        Task<TblUrlDetail?> GetUrlById(int id, CancellationToken cancellationToken);
        Task<bool> UpdateLinkRequest(TblUrlDetail tblUrlDetail, CancellationToken cancellationToken);
        Task<int> GetLatestUserPlanAsync(string userId, CancellationToken ct);
        Task<int> GetMonthlyCreatedLinkCount(string userId, CancellationToken ct);
        Task<int> GetMonthlyCreatedQRCount(string userId, CancellationToken cancellationToken);
        Task<List<TblQRDetail>> GetQRsCreatedBy(string userId, CancellationToken cancellationToken);
        Task<TblQRDetail?> GetQRById(int id, CancellationToken cancellationToken);
        int SaveQRRequest(TblQRDetail tblQRDetail);
        Task<bool> UpdateQRRequest(TblQRDetail tblQRDetail, CancellationToken cancellationToken);
        Task<PricingLimitDto?> GetPricingLimitAsync(int planId, int resourceId, CancellationToken cancellationToken);
        
    }
}
