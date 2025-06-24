using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Geonorge.Download.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Geonorge.Download.Controllers
{
    [Route("[controller]")]
    public class HomeController(ILogger<HomeController> logger) : Controller
    {

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var returnUrl = HttpContext.Session.GetString("ReturnUrl");
            if (!string.IsNullOrEmpty(returnUrl))
            {
                HttpContext.Session.Remove("ReturnUrl");
                logger.LogInformation("Redirect to ReturnUrl: {Url}", returnUrl);
                return Redirect(returnUrl);
            }

            ViewBag.Title = "Download";
            return View();
        }

        [HttpGet("signin")]
        public IActionResult SignIn()
        {
            string redirectUri = Url.Action(nameof(Index), "Home") ?? "/";

            var queryReturnUrl = HttpContext.Request.Query["ReturnUrl"].ToString();
            if (!string.IsNullOrEmpty(queryReturnUrl))
            {
                redirectUri = queryReturnUrl;
                HttpContext.Session.SetString("ReturnUrl", redirectUri);
                logger.LogInformation("Setting session ReturnUrl: {Url}", redirectUri);
            }

            var props = new AuthenticationProperties { RedirectUri = redirectUri };

            return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("signout")]
        public override SignOutResult SignOut()
        {
            // In ASP.NET Core, use IConfiguration instead of WebConfigurationManager
            var redirectUri = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()
                .GetValue<string>("GeoID:PostLogoutRedirectUri");

            var props = new AuthenticationProperties { RedirectUri = redirectUri };

            return SignOut(
                props,
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
        }

        [HttpGet("/signout-callback-oidc")]
        public IActionResult SignOutCallback()
        {
            return Redirect("/Home?logout=true");
        }

        [HttpPost("set-culture")]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            culture = CultureHelper.GetImplementedCulture(culture);

            Response.Cookies.Append("_culture", culture, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        [NonAction]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }

        [NonAction]
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                logger.LogError(context.Exception, "Unhandled exception");
                context.ExceptionHandled = true;
                context.Result = RedirectToAction("Index");
            }

            base.OnActionExecuted(context);
        }
    }
}
