using Azure.Core;
using Entities.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Models.DbModels;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Web.Helpers;
using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNet.Identity;
using System.Net;
using NuGet.Common;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace SportsMVC.Controllers
{
    public class UserController : BaseController
    {
        private readonly HttpClient client;
        private CrudStatus response;
        public new const string SessionKey = "Token";
        public new const string SessionId = "Id";
        private readonly IConfiguration _config;
        private ClaimsPrincipal principal;


        public UserController(IHttpClientFactory clientFactory, IConfiguration config)
        {
            principal = new ClaimsPrincipal();
            _config = config;
            response = new CrudStatus();
            client = clientFactory.CreateClient("someClient");
        }

        // GET: UserController
        public ActionResult Index()
        {
            string? token = HttpContext.Session.GetString(SessionKey);
            IEnumerable<UserDisplay> users = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var responseTask = client.GetAsync("User/UserDetails");
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readJob = result.Content.ReadFromJsonAsync<IList<UserDisplay>>();
                readJob.Wait();
                users = readJob.Result!;
            }
            else
            {
                users = Enumerable.Empty<UserDisplay>();
                ModelState.AddModelError(string.Empty, "server error");
            }

            return View(users);
        }

        public ActionResult Profile()
        {
            string? token = GetToken(SessionKey);
            int? id = GetId(SessionId);
            UserDisplay users = null!;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var responseTask = client.GetAsync("User/MyDetails?id="+id);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readJob = result.Content.ReadFromJsonAsync<UserDisplay>();
                readJob.Wait();
                users = readJob.Result!;
                return View(users);
            }
            ModelState.AddModelError(string.Empty, "server error");
            return View(users);
        }

        public ActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        [ActionName("LogIn")]
        [ValidateAntiForgeryToken]
        public ActionResult LogIn(LogIn user)
        {
            try
            {
                var postJob = client.PostAsJsonAsync<LogIn>("User/LogIn",user);
                postJob.Wait();
                var postResult =postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage, jsonSettings)!;
                if (postResult.IsSuccessStatusCode)
                {
                    if (response.Status == true)
                    {
                        HttpContext.Session.SetString(SessionKey, response.Message!);
                        HttpContext.Session.SetInt32(SessionId, Convert.ToInt32(response.Id!));
                        principal = ValidateToken(response.Message!,_config);
                        HttpContext.SignInAsync(principal);
                        return RedirectToAction("Profile", "User");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return View(user);
                    }
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View(user);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Register")]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Registration user)
        {
            try
            {
                var postJob = client.PostAsJsonAsync<Registration>("User/Registration", user);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage, jsonSettings)!;
                if (postResult.IsSuccessStatusCode)
                {
                    if(response.Status==true)
                    {
                        HttpContext.Session.SetString(SessionKey, response.Message!);
                        HttpContext.Session.SetInt32(SessionId, Convert.ToInt32(response.Id!));
                        principal = ValidateToken(response.Message!, _config);
                        HttpContext.SignInAsync(principal);
                        return RedirectToAction("Profile", "User");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return View(user);
                    }
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View(user);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Notifications()
        {
            int? id = HttpContext.Session.GetInt32(SessionId);
            IEnumerable<String> notifications = null!;
            string? token = HttpContext.Session.GetString(SessionKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var responseTask = client.GetAsync("User/UserNotifications?id="+id);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readJob = result.Content.ReadFromJsonAsync<IList<String>>();
                readJob.Wait();
                notifications = readJob.Result!;
            }
            else
            {
                notifications = Enumerable.Empty<String>();
                ModelState.AddModelError(string.Empty, "server error");
            }
            return View(notifications);
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ActionName("ForgotPassword")]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPassword newPassword)
        {
            try
            {
                var postJob = client.PutAsJsonAsync<ForgotPassword>("User/ForgetPassword", newPassword);
                postJob.Wait();
                var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage,jsonSettings)!;
                if (postResult.IsSuccessStatusCode)
                {

                    if (response.Status == true)
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return RedirectToAction("LogIn", "User");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return View(newPassword);
                    }
                }
                ModelState.AddModelError(string.Empty, "server Error");
                return View(newPassword);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult ChangeActiveStatus()
        {
            int? id = HttpContext.Session.GetInt32(SessionId);
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, client.BaseAddress + "User/Changing_Active_Status?id="+id);
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
                    return RedirectToAction("Profile", "User");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.Message!);
                    return View();
                }
            }
            ModelState.AddModelError(string.Empty, "server Error");
            return View("Profile");
        }

        public ActionResult LogOut()
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, client.BaseAddress + "User/LogOut");
            string? token = HttpContext.Session.GetString(SessionKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var postJob = client.SendAsync(req);
            postJob.Wait();
            var postResult = postJob.Result;
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync();
            return RedirectToAction("LogIn");
        }
    }
}
