﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Models.DbModels;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SportsMVC.Controllers
{
    public class BaseController : Controller
    {
        //private readonly HttpClient client;
        //readonly ITokenAcquisition _tokenAcquisition;
        public const string SessionKey = "UserId";
        readonly string clientId = string.Empty;
        readonly string _issuer = string.Empty;
        readonly string _audience = string.Empty;

        public BaseController(/*, ITokenAcquisition tokenAcquisition*/)
        {
            //client = factory.CreateClient("myApi");
            //_tokenAcquisition = tokenAcquisition;
            //client = new HttpClient();
            //client.BaseAddress = new Uri("https://localhost:7191/api/");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public string? GetToken(string sessionkey)
        {
            var test = HttpContext.Session.GetString(sessionkey);
            return test;
        }

        public static ClaimsPrincipal ValidateToken(string jwtToken)
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);

            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken;
            TokenValidationParameters validationParameters = new TokenValidationParameters();

            validationParameters.ValidateLifetime = true;
            var identity = new ClaimsPrincipal(new ClaimsIdentity(token.Claims));

            //validationParameters.ValidAudience = _audience.ToLower();
            //validationParameters.ValidIssuer = _issuer.ToLower();
            //validationParameters.IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes("s",3,323,8));

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);


            return principal;
        }

    }
}