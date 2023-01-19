using Azure;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DbModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;

namespace SportsMVC.Controllers
{
    public class GroundController : BaseController
    {
        private readonly HttpClient client;
        private CrudStatus response;
        public new const string SessionKey = "Token";
        public GroundController(IHttpClientFactory clientFactory)
        {
            response = new CrudStatus();
            client = clientFactory.CreateClient("someClient");
        }
        // GET: GroundController
        public ActionResult Index(string city)
        {
            IEnumerable<GroundList> grounds = null!;
            string? token = HttpContext.Session.GetString(SessionKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                   parameter: token);
            if (city==null)
            {
                var responseTask = client.GetAsync("Ground/GetGroundDetails");
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readJob = result.Content.ReadFromJsonAsync<IList<GroundList>>();
                    readJob.Wait();
                    grounds = readJob.Result!;
                }
                else
                {
                    grounds = Enumerable.Empty<GroundList>();
                    ModelState.AddModelError(string.Empty, "server error");
                }

                return View(grounds);
            }
            else
            {
                grounds = Enumerable.Empty<GroundList>();
                var postJob = client.GetAsync("Ground/Ground_City?City=" + city);
                postJob.Wait();
                var postResult = postJob.Result;
                if (postResult.IsSuccessStatusCode)
                {
                    var readJob = postResult.Content.ReadFromJsonAsync<IList<GroundList>>();
                    readJob.Wait();
                    grounds = readJob.Result!;
                    return View(grounds);
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View(grounds);
            }
        }

        public ActionResult MyGround()
        {
            string? token = HttpContext.Session.GetString(SessionKey);
            IEnumerable<GroundList> grounds = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var postJob = client.GetAsync("Ground/MyGrounds");
            postJob.Wait();
            var postResult = postJob.Result;
            if (postResult.IsSuccessStatusCode)
            {
                var readJob = postResult.Content.ReadFromJsonAsync<IEnumerable<GroundList>>();
                readJob.Wait();
                grounds = readJob.Result!;
                return View(grounds);
            }
            else
            {
                grounds = Enumerable.Empty<GroundList>();
                ModelState.AddModelError(string.Empty, "server error");
            }
            return View(grounds);
        }

        public ActionResult SearchByGroundName(string ground)
        {
            if (ground == null)
            {
                return RedirectToAction("Index", "Ground");
            }
            string? token = HttpContext.Session.GetString(SessionKey);
            GroundList grounds = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var postJob = client.GetAsync("Ground/GroundName?GroundName=" + ground);
            postJob.Wait();
            var postResult = postJob.Result;
            if (postResult.IsSuccessStatusCode)
            {
                var readJob = postResult.Content.ReadFromJsonAsync<GroundList>();
                readJob.Wait();
                grounds = readJob.Result!;
                return View(grounds);
            }
            ModelState.AddModelError(string.Empty, "server Error");
            return View(grounds);
        }

        public ActionResult ChangeActiveStatus(int groundId)
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, client.BaseAddress + "Ground/Changing_Active_Status?groundID=" + groundId);
            string? token = HttpContext.Session.GetString(SessionKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var postJob = client.SendAsync(req);
            postJob.Wait();
            var postResult = postJob.Result;
            var resultMessage = postResult.Content.ReadAsStringAsync().Result;
            response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage)!;
            if (postResult.IsSuccessStatusCode)
            {
                if (response.Status == true)
                {
                    ModelState.AddModelError(string.Empty, response.Message!);
                    return RedirectToAction("MyGround", "Ground");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.Message!);
                    return RedirectToAction("MyGround", "Ground");
                }
            }
            ModelState.AddModelError(string.Empty, "server Error");
            return View("MyGround");
        }
    }
}
