using KurzUrl.Repository.Models;
using System.Threading;
using System.Threading.Tasks;

namespace KurzUrl.BusinessLayer.Interface
{
    public interface IQRService
    {
        string GenerateQR(string url);
        Task<List<TblQRDetail>> GetQRsCreatedByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task<SaveQRResult> SaveQRAsync(SaveQRRequest request, string userId, CancellationToken cancellationToken);
        Task<UpdateQRResult> UpdateQRAsync(UpdateQRRequest request, string userId, CancellationToken cancellationToken);
    }

    public class SaveQRResult
    {
        public bool Success { get; set; }
        public int? Id { get; set; }
        public string? MainUrl { get; set; }
        public byte[]? QRImage { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? Message { get; set; }
        public PlanLimitReached? PlanLimitReached { get; set; }
    }

    public class UpdateQRResult
    {
        public bool Success { get; set; }
        public int? Id { get; set; }
        public string? MainUrl { get; set; }
        public string? Title { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? Message { get; set; }
    }
}

