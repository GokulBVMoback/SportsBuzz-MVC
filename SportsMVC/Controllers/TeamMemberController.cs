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

        public ActionResult MyTeamMember(int teamId)
        {
            string? token = HttpContext.Session.GetString(SessionKey);
            IEnumerable<PlayerList> teams = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
             parameter: token);
            var responseTask = client.GetAsync("TeamMember/MyTeamMembers?teamId="+teamId);
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

        public ActionResult AddPlayer(int teamId)
        {
            HttpContext.Session.SetInt32(SessionTeamId,teamId);
            return View();
        }

        [HttpPost]
        [ActionName("AddPlayer")]
        [ValidateAntiForgeryToken]
        public ActionResult AddPlayer(PlayerRegister player)
        {
            try
            {
                player.TeamId= HttpContext.Session.GetInt32(SessionTeamId);
                string? token = HttpContext.Session.GetString(SessionKey);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                    parameter: token);
                var postJob = client.PostAsJsonAsync<PlayerRegister>("TeamMember/AddTeamMember", player);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage, jsonSettings)!;
                if (postResult.IsSuccessStatusCode)
                {
                    if (response.Status == true)
                    {
                        return RedirectToAction("MyTeam", "Team");
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
                string? token = HttpContext.Session.GetString(SessionKey);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                    parameter: token);
                var postJob = client.PutAsJsonAsync<EditPlayer>("TeamMember/EditTeamMember", player);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage, jsonSettings)!;
                if (postResult.IsSuccessStatusCode)
                {

                    if (response.Status == true)
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        id = Convert.ToInt32(HttpContext.Session.GetInt32(SessionTeamId));
                        return RedirectToAction("MyTeam", "Team");
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
            string? token = HttpContext.Session.GetString(SessionKey);
            PlayerList player = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
             parameter: token);
            var responseTask = client.GetAsync("TeamMember/TeamMembersById?playerID=" + id);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readJob = result.Content.ReadFromJsonAsync<PlayerList>();
                readJob.Wait();
                player = readJob.Result!;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "server error");
            }
            return View(player);
        }

        // POST: TeamMemberController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                string? token = HttpContext.Session.GetString(SessionKey);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                    parameter: token);
                var postJob = client.DeleteAsync("TeamMember/delete?memberID="+id);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage, jsonSettings)!;
                if (postResult.IsSuccessStatusCode)
                {

                    if (response.Status == true)
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return RedirectToAction("MyTeam", "Team");
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
