using KurzUrl.Repository.Models;
using System.Threading;
using System.Threading.Tasks;

namespace KurzUrl.BusinessLayer.Interface
{
    public interface IUserLinksService
    {
        Task<List<TblUrlDetail>> GetLinksCreatedByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task<CreateLinkResult> CreateLinkAsync(CreateLinkRequest request, string userId, CancellationToken cancellationToken);
        Task<UpdateLinkResult> UpdateLinkAsync(UpdateLinkRequest request, string userId, CancellationToken cancellationToken);
    }

    public class CreateLinkResult
    {
        public bool Success { get; set; }
        public int? Id { get; set; }
        public string? MainUrl { get; set; }
        public string? ShortUrl { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? Message { get; set; }
        public PlanLimitReached? PlanLimitReached { get; set; }
    }

    public class UpdateLinkResult
    {
        public bool Success { get; set; }
        public int? Id { get; set; }
        public string? MainUrl { get; set; }
        public string? Title { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? Message { get; set; }
    }
}

