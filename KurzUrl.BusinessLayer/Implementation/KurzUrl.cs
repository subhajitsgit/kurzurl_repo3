using KurzUrl.BusinessLayer.Interface;
using KurzUrl.BusinessLayer.ViewModel;
using KurzUrl.Repository.Implementation;
using KurzUrl.Repository.Interface;
using KurzUrl.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KurzUrl.BusinessLayer.Implementation
{
    public class KurzUrl : IKurzUrl
    {
        public IKurzUrlRepo? KurzUrlRepo { get; set; }
        public ShortUrlContext ShortUrlContext { get; set; }
        public KurzUrl(IKurzUrlRepo? kurzUrlRepo)
        {
            KurzUrlRepo = kurzUrlRepo;
        }
        private static Random random = new Random();
        public string GenerateKurzUrl()
        {
            var shortUrlDomain = "https://kurzurl.de/";
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var randomValue = new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return shortUrlDomain + randomValue;
        }

        public ApiResponse GetKurzUrlDetails(int id)
        {
            var response = new ApiResponse();
            //var repo = new KurzUrlRepo();
            var details = KurzUrlRepo?.GetKurzUrlDetail(id);
            if (details != null)
            {
                response.success = true;
                response.data = details.ShortUrl;
                response.message = "Success";
            }
            else
            {
                response.success = false;
                response.message = "Error";
            }
            return response;
        }

        public ApiResponse SaveKurzUrl(PostData data)
        {
            var response = new ApiResponse();
            //var repo = new KurzUrlRepo();
            var tblDetails = new TblUrlDetail();
            tblDetails.ShortUrl = GenerateKurzUrl();
            tblDetails.MainUrl = data.url;
            tblDetails.CreatedOn = DateTime.Now;
            tblDetails.CreatedBy = "jit";
            var id = KurzUrlRepo?.SaveKurzUrl(tblDetails);
            if (id > 0)
            {
                response.success = true;
                response.data = id;
                response.message = "Success";
            }
            else
            {
                response.success = false;
                response.data = 0;
                response.message = "Error";
            }
            return response;
        }

        public async Task<string> GetMainUrl(string shortUrl)
        {
            try
            {
                var rtn = await KurzUrlRepo.GetMainUrlFromShortUrl(shortUrl);
                if (rtn is null)
                    throw new KeyNotFoundException("Short Url Was Not Valid");

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}