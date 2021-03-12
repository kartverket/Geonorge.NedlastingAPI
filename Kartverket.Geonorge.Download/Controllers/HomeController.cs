using System;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Kartverket.Geonorge.Download.Helpers;
using log4net;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

namespace Kartverket.Geonorge.Download.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Index()
        {
            if (Session["ReturnUrl"] != null)
            {
                var redirectUrl = Session["ReturnUrl"].ToString();
                Session["ReturnUrl"] = null;
                Log.Info("Redirect to ReturnUrl:" + redirectUrl);
                Response.Redirect(redirectUrl);
            }
                ViewBag.Title = "Download";

            return View();
        }

        public void SignIn()
        {
            var redirectUri = Url.Action(nameof(HomeController.Index), "Home");

            if (Request.QueryString["ReturnUrl"] != null) {
                redirectUri = Request.QueryString["ReturnUrl"];
                Session["ReturnUrl"] = redirectUri;
                Log.Info("Setting session ReturnUrl:" + Session["ReturnUrl"]);
            }

            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUri },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public void SignOut()
        {
            var redirectUri = WebConfigurationManager.AppSettings["GeoID:PostLogoutRedirectUri"];
            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties {RedirectUri = redirectUri},
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);
        }

        /// <summary>
        /// This is the action responding to /signout-callback-oidc route after logout at the identity provider
        /// </summary>
        /// <returns></returns>
        public ActionResult SignOutCallback()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public ActionResult SetCulture(string culture, string returnUrl)
        {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);
            // Save culture in a cookie
            var cookie = Request.Cookies["_culture"];
            if (cookie != null)
            {
                cookie.Value = culture; // update cookie value
            }
            else
            {
                cookie = new HttpCookie("_culture");
                cookie.Value = culture;
                cookie.Expires = DateTime.Now.AddYears(1);
            }
            Response.Cookies.Add(cookie);

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Log.Error("Error", filterContext.Exception);
        }
    }
}