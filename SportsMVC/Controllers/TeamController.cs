using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DbModels;
using System.Diagnostics;
using System.Net.Http.Headers;

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
        public ActionResult Index()
        {
            IEnumerable<TeamList> teams = null!;

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

        public ActionResult SearchByTeamName(string team)
        {
            string? token = HttpContext.Session.GetString(SessionKey);
            TeamList teams = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var postJob = client.GetAsync("Team/Team?Team=" + team);
            postJob.Wait();
            var postResult = postJob.Result;
            //var resultMessage = postResult.Content.ReadAsStringAsync().Result;
            //ground = JsonConvert.DeserializeObject<GroundList>(resultMessage)!;
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

        // GET: TeamController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TeamController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TeamController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TeamController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TeamController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TeamController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TeamController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
