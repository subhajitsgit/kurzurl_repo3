using KurzUrl.BusinessLayer.ViewModel;
using KurzUrl.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurzUrl.BusinessLayer.Interface
{
    public interface IKurzUrl
    {
        string GenerateKurzUrl();
        ApiResponse GetKurzUrlDetails(int id);
        ApiResponse SaveKurzUrl(PostData data);
        Task<string> GetMainUrl(string shortUrl);

    }
}
