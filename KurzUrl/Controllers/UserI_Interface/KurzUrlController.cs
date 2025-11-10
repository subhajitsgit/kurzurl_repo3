using KurzUrl.Repository.Interface;
using KurzUrl.BusinessLayer.Interface;
using KurzUrl.BusinessLayer.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using KurzUrl.Repository.Models;
using System.Net;
using System.Security.Policy;

namespace KurzUrl.Controllers.User_Interface
{
    [Route("api/[controller]/action")]
    [ApiController]
    public class KurzUrlController : ControllerBase
    {
        public IKurzUrlRepo _kurzUrlRepo { get; set; }
        public IKurzUrl _kurzUrl { get; set; }
        public IHttpContextAccessor _HttpContextAccessor { get; set; }
        public KurzUrlController(IKurzUrlRepo kurzUrlRepo, IKurzUrl kurzUrl, IHttpContextAccessor httpContextAccessor)
        {
            _HttpContextAccessor = httpContextAccessor;
            _kurzUrlRepo = kurzUrlRepo;
            _kurzUrl = kurzUrl;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<KurzUrlController>/5
        [HttpGet("{id}")]
        public ApiResponse Get(int id)
        {
            var response = _kurzUrl.GetKurzUrlDetails(id);
            return response;
        }

        // POST api/<KurzUrlController>
        [HttpPost]
        public ApiResponse Post([FromBody] PostData data)
        {
            var response = new ApiResponse();
            response = _kurzUrl.SaveKurzUrl(data);
            return response;
        }

        // PUT api/<KurzUrlController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<KurzUrlController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        /// <summary>
        /// you will be redirect to another site according to inserted shorturl .
        /// </summary>
        /// <param name="inputShortUrl">Insert your shortUrl that given you before!</param>
        /// <returns></returns>
         [HttpGet(nameof(RedirectToAnotherSite))]
        public async Task<IActionResult> RedirectToAnotherSite(string inputShortUrl)
        {
            try
            {
                var result = await _kurzUrl.GetMainUrl(inputShortUrl);
                Console.WriteLine(result);
                //Response.Redirect("https://" + result, permanent: false);
                //return new EmptyResult();
                return Ok(new { url = "https://" + result });
            }
            catch (Exception ex)
            {
                if (ex is KeyNotFoundException)
                    return StatusCode(404, "ShortUrl Is Not Valid");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
