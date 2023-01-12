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

namespace SportsMVC.Controllers
{
    public class UserController : BaseController
    {
        private readonly HttpClient client;
        private CrudStatus response;
        public new const string SessionKey = "Token";

        public UserController(IHttpClientFactory clientFactory)
        {
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
            var responseTask = client.GetAsync("User/");
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
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage)!;
                HttpContext.Session.SetString(SessionKey, response.Message!);
                string token=GetToken(SessionKey)!;
                if (postResult.IsSuccessStatusCode)
                {
                    if (response.Status == true)
                    {
                        var token2 = new JwtSecurityTokenHandler().ReadJwtToken(token);
                        var identity = new ClaimsPrincipal(new ClaimsIdentity(token2.Claims));
                        var identity2 = User.Identity as ClaimsIdentity;
                        identity2.AddClaims(token2.Claims);

                        //ClaimsIdentity identity3 = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                        //AuthenticationManager.SignIn(new AuthenticationProperties()
                        //{
                        //    IsPersistent = isPersistent
                        //}, identity3);
                        var identity4 = HttpContext.User.Identity as ClaimsIdentity;
                        if (identity!= null) 
                        {
                            HttpContext.User.AddIdentity(identity2);
                            IEnumerable<Claim> claims = identity.Claims;
                        }
                        //var principal =ValidateToken(token);
                        //HttpContext.SignInAsync(principal);
                        return RedirectToAction("Index", "User");
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
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage)!;
                if (postResult.IsSuccessStatusCode)
                {
                    if(response.Status==true)
                    {
                        return RedirectToAction("Index", "User");
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
            IEnumerable<String> notifications = null!;
            string? token = HttpContext.Session.GetString(SessionKey);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                parameter: token);
            var responseTask = client.GetAsync("User/UserNotifications");
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
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage)!;
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
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, client.BaseAddress + "User/Changing_Active_Status");
                string? token = HttpContext.Session.GetString(SessionKey);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "Bearer",
                    parameter: token);
                var postJob = client.SendAsync(req);
                postJob.Wait();
                var postResult = postJob.Result;
                //postJob.Wait();
                //var postResult = postJob.Result;
                var resultMessage = postResult.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<CrudStatus>(resultMessage)!;
            ViewBag["msg"] = response.Message;

            if (postResult.IsSuccessStatusCode)
                {
                    if (response.Status == true)
                    {
                    ViewBag["msg"]= response.Message;
                        ModelState.AddModelError(string.Empty, response.Message!);
                        return RedirectToAction("Index", "User");
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
    }
}
