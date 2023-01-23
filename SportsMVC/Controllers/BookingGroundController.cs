using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Models.DbModels;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace SportsMVC.Controllers
{
    public class BookingGroundController : BaseController
    {
        private readonly HttpClient client;
        private CrudStatus response;
        public new const string SessionKey = "Token";
        public new const string SessionGrndId = "grndId";

        public BookingGroundController(IHttpClientFactory clientFactory)
        {
            response = new CrudStatus();
            client = clientFactory.CreateClient("someClient");
        }
        // GET: GroundController
        public ActionResult Index()
        {
            IEnumerable<BookedGroundView> grounds = null!;
            string? token = HttpContext.Session.GetString(SessionKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                   parameter: token);
                var responseTask = client.GetAsync("BookingGround/BookingDetails");
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readJob = result.Content.ReadFromJsonAsync<IList<BookedGroundView>>();
                    readJob.Wait();
                    grounds = readJob.Result!;
                }
                else
                {
                    grounds = Enumerable.Empty<BookedGroundView>();
                    ModelState.AddModelError(string.Empty, "server error");
                }
                return View(grounds);   
        }

        public ActionResult MyBooking()
        {
            int? id = GetId(SessionId);
            string? token = HttpContext.Session.GetString(SessionKey);
            IEnumerable<BookedGroundView> grounds = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var postJob = client.GetAsync("BookingGround/MyBookingDetails?id=" + id);
            postJob.Wait();
            var postResult = postJob.Result;
            if (postResult.IsSuccessStatusCode)
            {
                var readJob = postResult.Content.ReadFromJsonAsync<IEnumerable<BookedGroundView>>();
                readJob.Wait();
                grounds = readJob.Result!;
                return View(grounds);
            }
            else
            {
                grounds = Enumerable.Empty<BookedGroundView>();
                ModelState.AddModelError(string.Empty, "server error");
            }
            return View(grounds);
        }

        public ActionResult GetAvailableGround()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetAvailableGround(SearchAvailableGround availableGround)
        {
            return RedirectToAction("ReturnGround", availableGround);
        }

        public ActionResult ReturnGround(SearchAvailableGround availableGround)
        {
            IEnumerable<GroundList> grounds = null!;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress + "BookingGround/GetAvailableGroundDetails");
            req.Content = new StringContent(JsonConvert.SerializeObject(availableGround), Encoding.UTF8, "application/json");
            string? token = HttpContext.Session.GetString(SessionKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                   parameter: token);
            var postJob = client.SendAsync(req);
            postJob.Wait();
            var result = postJob.Result;
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
        //public ActionResult BookingGround()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ActionName("BookingGround")]
        //[ValidateAntiForgeryToken]
        public ActionResult BookingGround(GroundBooking availableground)
        {
            try
            {
                string? token = GetToken(SessionKey);
                int? id = GetId(SessionId);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
                var postJob = client.PostAsJsonAsync<GroundBooking>("BookinGround/Booking_Match", availableground);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage, jsonSettings)!;
                if (postResult.IsSuccessStatusCode)
                {
                    if (response.Status == true)
                    {
                        return RedirectToAction("MyBooking");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return View("ReturnGround", availableground);
                    }
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View("ReturnGround", availableground);
            }
            catch
            {
                return View();
            }
        }
    }
}
