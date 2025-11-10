using KurzUrl.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace KurzUrl.Data.Dto
{
    public class CreateCheckoutDto
    {
        public string Name { get; set; }
        public string Currency { get; set; } = "usd";

        [Range(0, long.MaxValue)]
        public long AmountInCents { get; set; }

        [Url]
        public string SuccessUrl { get; set; }

        [Url]
        public string CancelUrl { get; set; }
    }
}
