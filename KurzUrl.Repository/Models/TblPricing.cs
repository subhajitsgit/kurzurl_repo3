using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Repository.Models
{
    [Table("tbl_Pricing", Schema = "dbo")]
    public partial class TblPricing
    {
        public int Id { get; set; }
        public string? Weight { get; set; }
    }
}
