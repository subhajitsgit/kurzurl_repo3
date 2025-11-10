using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Repository.Models
{
    [Table("tblPricingLimit", Schema = "dbo")]
    public partial class TblPricingLimit
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public int Limit {  get; set; }
        public int UpgradeToPlanId { get; set; }
        public int ResourceId { get; set; }
    }
}
