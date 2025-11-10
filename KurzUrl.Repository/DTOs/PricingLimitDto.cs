using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Repository.DTOs
{
    public class PricingLimitDto
    {
        public int PlanId { get; set; }
        public string ResourceType { get; set; } = string.Empty;
        public int Limit { get; set; }
        public string? UpgradeToPlanName { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string PlanType {  get; set; } = string.Empty;
    }
}
