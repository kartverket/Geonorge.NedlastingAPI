using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

namespace Geonorge.Download.Controllers
{
    [AllowAnonymous]
    [Route("account")]
    public class AccountController(ILogger<AccountController> logger, IConfiguration config) : Controller
    {
        // GET /account/login
        [HttpGet("login")]
        public IActionResult Login(string returnUrl = "/")
        {
            var redirectUri = returnUrl; // string.IsNullOrWhiteSpace(config["auth:oidc:RedirectUri"]) ? "/" : config["auth:oidc:RedirectUri"];
            
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = redirectUri
            }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        // GET /account/logout
        [HttpGet("logout")]
        public async Task<IActionResult> SignOut()
        {
            var redirectUri = string.IsNullOrWhiteSpace(config["auth:oidc:PostLogoutRedirectUri"]) ? "/" : config["auth:oidc:PostLogoutRedirectUri"];
            var cookieAuth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!cookieAuth.Succeeded)
                // Already logged out locally
                return Redirect(redirectUri!);

            // Try to get the OIDC authenticate result for tokens (requires SaveTokens = true)
            var oidcAuth = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
            var idToken = oidcAuth.Properties?.GetTokenValue("id_token");

            var props = new AuthenticationProperties { RedirectUri = redirectUri };

            // If id_token exists, store it so the OIDC handler can send id_token_hint.
            if (!string.IsNullOrEmpty(idToken))
                props.StoreTokens([new AuthenticationToken { Name = "id_token", Value = idToken }]);

            return SignOut(props, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("set-culture")]
        public IActionResult SetCulture(string culture, string returnUrl = "/")
        {
            Response.Cookies.Append(
                "_culture",
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    Path = "/"
                });

            return Redirect(returnUrl);
        }
    }
}
