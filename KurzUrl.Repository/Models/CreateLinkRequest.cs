
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Repository.Models
{
    public class CreateLinkRequest
    {
        public int UserId { get; set; }
        public string MainUrl { get; set; } = string.Empty;
        public string? Title { get; set; }
    }
}
