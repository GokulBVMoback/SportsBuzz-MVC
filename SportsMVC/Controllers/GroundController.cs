using Azure;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DbModels;
using Newtonsoft.Json;
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
        public ActionResult Index()
        {
            IEnumerable<GroundList> grounds = null!;

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

        public ActionResult SearchByGroundCity(string city)
        {
            string? token = HttpContext.Session.GetString(SessionKey);
            IEnumerable<GroundList> grounds = null!;
            grounds = Enumerable.Empty<GroundList>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
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
        
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TeamMemberController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                var postJob = client.DeleteAsync("TeamMember/delete?memberID=" + id);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage)!;
                if (postResult.IsSuccessStatusCode)
                {

                    if (response.Status == true)
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return RedirectToAction("Index", "TeamMember");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return View();
                    }
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View();
            }
            catch
            {
                return View();
            }
        }

        //[HttpPost]
        //[ActionName("SearchByGroundCity")]
        //[ValidateAntiForgeryToken]
        //public ActionResult SearchByGroundCity(string city)
        //{
        //    try
        //    {
        //        IEnumerable<GroundList> grounds = null!;
        //        var postJob = client.PostAsJsonAsync<string>("Ground/SearchByGroundCity", city);
        //        postJob.Wait();
        //        var postResult = postJob.Result;
        //        //var resultMessage = postResult.Content.ReadAsStringAsync().Result;
        //        //ground = JsonConvert.DeserializeObject<GroundList>(resultMessage)!;
        //        if (postResult.IsSuccessStatusCode)
        //        {
        //            if (response.Status == true)
        //            {
        //                var readJob = postResult.Content.ReadFromJsonAsync<IList<GroundList>>();
        //                readJob.Wait();
        //                grounds = readJob.Result!;
        //                return View(grounds);
        //            }
        //            else
        //            {
        //                ModelState.AddModelError(string.Empty, response.Message!);
        //                return View(city);
        //            }
        //        }
        //        ModelState.AddModelError(string.Empty, "server Error");
        //        return View(city);
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
