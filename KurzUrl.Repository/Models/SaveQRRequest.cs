using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.Repository.Models
{
    public class SaveQRRequest
    {
        public string MainUrl { get; set; }
        public string QRBase64 {  get; set; }
        public string Title { get; set; }
    }
}
