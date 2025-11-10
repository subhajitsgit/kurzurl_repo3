using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.BusinessLayer.ViewModel
{
    public class ApiResponse
    {
        public Boolean success { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
}
