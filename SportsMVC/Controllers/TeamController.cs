using Azure;
using Entities.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DbModels;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace SportsMVC.Controllers
{
    public class TeamController : BaseController
    {
        private readonly HttpClient client;
        private CrudStatus response;
        public new const string SessionKey = "Token";


        public TeamController(IHttpClientFactory clientFactory)
        {
            response= new CrudStatus();
            client = new HttpClient();
            client = clientFactory.CreateClient("someClient");
        }
        // GET: TeamController
        public ActionResult Index(string city)
        {
            IEnumerable<TeamList> teams = null!;
            string? token = HttpContext.Session.GetString(SessionKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            if (city==null)
            {
                var responseTask = client.GetAsync("Team/");
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readJob = result.Content.ReadFromJsonAsync<IList<TeamList>>();
                    readJob.Wait();
                    teams = readJob.Result!;
                }
                else
                {
                    teams = Enumerable.Empty<TeamList>();
                    ModelState.AddModelError(string.Empty, "server error");
                }
                return View(teams);
            }
            else
            {
                var postJob = client.GetAsync("Team/City?City=" + city);
                postJob.Wait();
                var postResult = postJob.Result;
                if (postResult.IsSuccessStatusCode)
                {
                    var readJob = postResult.Content.ReadFromJsonAsync<IList<TeamList>>();
                    readJob.Wait();
                    teams = readJob.Result!;
                    return View(teams);
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View(teams);
            }
        }

        public ActionResult SearchByTeamName(string team)
        {
            if(team==null)
            {
                return RedirectToAction("Index", "Team");
            }
            string? token = HttpContext.Session.GetString(SessionKey);
            TeamList teams = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var postJob = client.GetAsync("Team/Team?Team=" + team);
            postJob.Wait();
            var postResult = postJob.Result;
            if (postResult.IsSuccessStatusCode)
            {
                    var readJob = postResult.Content.ReadFromJsonAsync<TeamList>();
                    readJob.Wait();
                    teams = readJob.Result!;
                    return View(teams);
            }
            ModelState.AddModelError(string.Empty, "server Error");
            return View(teams);
        }

        public ActionResult MyTeam()
        {
            string? token = HttpContext.Session.GetString(SessionKey);
            IEnumerable<TeamList> teams = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var postJob = client.GetAsync("Team/MyTeams");
            postJob.Wait();
            var postResult = postJob.Result;
            if (postResult.IsSuccessStatusCode)
            {
                var readJob = postResult.Content.ReadFromJsonAsync<IEnumerable<TeamList>>();
                readJob.Wait();
                teams = readJob.Result!;
                return View(teams);
            }
            else
            {
                teams = Enumerable.Empty<TeamList>();
                ModelState.AddModelError(string.Empty, "server error");
            }
            return View(teams);
        }
        public ActionResult TeamRegister()
        {
            return View();
        }

        [HttpPost]
        [ActionName("TeamRegister")]
        [ValidateAntiForgeryToken]
        public ActionResult TeamRegister(TeamRegister team)
        {
            try
            {
                string? token = HttpContext.Session.GetString(SessionKey);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
                var postJob = client.PostAsJsonAsync<TeamRegister>("Team/TeamRegistration", team);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage)!;
                if (postResult.IsSuccessStatusCode)
                {
                    if (response.Status == true)
                    {
                        return RedirectToAction("MyTeam", "Team");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return View(team);
                    }
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View(team);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult ChangeActiveStatus(int teamId)
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, client.BaseAddress + "Team/Changing_Active_Status?teamID="+teamId);
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
                    return RedirectToAction("MyTeam", "Team");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.Message!);
                    return RedirectToAction("MyTeam", "Team");
                }
            }
            ModelState.AddModelError(string.Empty, "server Error");
            return View("MyTeam");
        }
    }
}
