using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DbModels;
using Newtonsoft.Json;
using NuGet.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Numerics;

namespace SportsMVC.Controllers
{
    public class TeamMemberController : BaseController
    {
        private readonly HttpClient client;
        private CrudStatus response;
        public new const string SessionKey = "Token";

        public TeamMemberController(IHttpClientFactory clientFactory)
        {
            response= new CrudStatus();
            client = new HttpClient();
            client = clientFactory.CreateClient("someClient");
        }
        
        // GET: TeamController
        public ActionResult Index()
        {
            string? token = HttpContext.Session.GetString(SessionKey);
            IEnumerable<PlayerList> teams = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
             parameter: token);
            var responseTask = client.GetAsync("TeamMember");
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readJob = result.Content.ReadFromJsonAsync<IList<PlayerList>>();
                readJob.Wait();
                teams = readJob.Result!;
            }
            else
            {
                teams = Enumerable.Empty<PlayerList>();
                ModelState.AddModelError(string.Empty, "server error");
            }

            return View(teams);
        }

        public ActionResult AddPlayer()
        {
            return View();
        }

        [HttpPost]
        [ActionName("AddPlayer")]
        [ValidateAntiForgeryToken]
        public ActionResult AddPlayer(PlayerRegister player)
        {
            try
            {
                var postJob = client.PostAsJsonAsync<PlayerRegister>("TeamMember/AddTeamMember", player);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage)!;
                if (postResult.IsSuccessStatusCode)
                {
                    if (response.Status == true)
                    {
                        return RedirectToAction("Index", "TeamMember");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return View(player);
                    }
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View(player);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult EditPlayer(int id)
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ActionName("EditPlayer")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPlayer(int id,EditPlayer player)
        {
            try
            {
                player.MemberId= id;
                var postJob = client.PutAsJsonAsync<EditPlayer>("TeamMember/EditTeamMember", player);
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
                        return View(player);
                    }
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View(player);
            }
            catch
            {
                return View();
            }
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
                var postJob = client.DeleteAsync("TeamMember/delete?memberID="+id);
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
    }
}
