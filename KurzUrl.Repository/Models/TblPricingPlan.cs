using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Repository.Models
{
    [Table("tblPricingPlan", Schema = "dbo")]
    public partial class TblPricingPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string PlanType { get; set; } = string.Empty;
    }
}
