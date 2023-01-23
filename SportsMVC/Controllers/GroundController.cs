using Azure;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DbModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SportsMVC.Controllers
{
    public class GroundController : BaseController
    {
        private readonly HttpClient client;
        private CrudStatus response;
        public new const string SessionKey = "Token";
        public new const string SessionGrndId = "grndId";

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
            int? id = GetId(SessionId);
            string? token = HttpContext.Session.GetString(SessionKey);
            IEnumerable<GroundList> grounds = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var postJob = client.GetAsync("Ground/MyGrounds?id="+id);
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

        public ActionResult GroundRegister()
        {
            return View();
        }

        [HttpPost]
        [ActionName("GroundRegister")]
        [ValidateAntiForgeryToken]
        public ActionResult GroundRegister(GroundRegister ground)
        {
            try
            {
                string? token = HttpContext.Session.GetString(SessionKey);
                int? id = GetId(SessionId);
                ground.UserId = id;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
                var postJob = client.PostAsJsonAsync<GroundRegister>("Ground/AddGround", ground);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage, jsonSettings)!;
                if (postResult.IsSuccessStatusCode)
                {
                    if (response.Status == true)
                    {
                        return RedirectToAction("MyGround", "Ground");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return View(ground);
                    }
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View(ground);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult EditGround(int groundId)
        {
            HttpContext.Session.SetInt32(SessionGrndId, groundId);
            return View();
        }

        [HttpPost]
        [ActionName("EditGround")]
        [ValidateAntiForgeryToken]
        public ActionResult EditGround(int groundId, EditGround ground)
        {
            try
            {
                groundId = (int)GetGrndId(SessionGrndId)!;
                ground.GroundId = groundId;
                string? token = GetToken(SessionKey);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                    parameter: token);
                var postJob = client.PutAsJsonAsync<EditGround>("Ground/EditGround", ground);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage, jsonSettings)!;
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
                        return View(ground);
                    }
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View(ground);
            }
            catch
            {
                return View();
            }
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
            response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage, jsonSettings)!;
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
