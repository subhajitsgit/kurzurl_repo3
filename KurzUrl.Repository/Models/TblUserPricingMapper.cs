using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Repository.Models
{
    [Table("tblUserPricingMapper", Schema = "dbo")]
    public partial class TblUserPricingMapper
    {
        public string Id { get; set; } = null!;
        [Column("UserId")]
        public string UserId { get; set; }

        [Column("PlanId")]
        public int PlanId { get; set; }

        [Column("CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [Column("CreatedBy")]
        [MaxLength(50)]
        public string? CreatedBy { get; set; }
    }
}
