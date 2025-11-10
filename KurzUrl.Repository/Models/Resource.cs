using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Repository.Models
{
    [Table("Resource", Schema = "dbo")]
    public partial class Resource
    {
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
    }
}
