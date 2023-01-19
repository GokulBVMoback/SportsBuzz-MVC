using Microsoft.AspNetCore.Authorization;
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
        public const string SessionKey = "UserId";

        public BaseController()
        {
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public string? GetToken(string sessionkey)
        {
            var test = HttpContext.Session.GetString(sessionkey);
            return test;
        }

        public static ClaimsPrincipal ValidateToken(string jwtToken,IConfiguration config)
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);

            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken;
            TokenValidationParameters validationParameters = new TokenValidationParameters();

            validationParameters.ValidateLifetime = true;
            var identity = new ClaimsPrincipal(new ClaimsIdentity(token.Claims));

            var TokenInfo = new Dictionary<string, string>();
            var claims = token.Claims.ToList();
            foreach (var claim in claims)
            {
                TokenInfo.Add(claim.Type, claim.Value);
                if (claim.Type == "aud")
                {
                    validationParameters.ValidAudience = claim.Value;
                }
                else if(claim.Type == "iss")
                {
                    validationParameters.ValidIssuer = claim.Value;
                }
            }
            validationParameters.IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);


            return principal;
        }

    }
}
