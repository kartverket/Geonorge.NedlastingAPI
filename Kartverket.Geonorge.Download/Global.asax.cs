using Kartverket.Geonorge.Download.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Kartverket.Geonorge.Download.App_Start;
using Kartverket.Geonorge.Download.Models.Translations;
using System.Collections.Specialized;

namespace Kartverket.Geonorge.Download
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WebApiApplication));

        protected void Application_Start()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DownloadContext, Migrations.Configuration>());
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("json", "true", "application/json"));
            log4net.Config.XmlConfigurator.Configure();

            HttpConfiguration config = GlobalConfiguration.Configuration;

            config.Formatters.JsonFormatter
                        .SerializerSettings
                        .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            MvcHandler.DisableMvcResponseHeader = true;

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();

            log.Error("App_Error", ex);
        }

        protected void Session_Start() { }

        protected void Application_BeginRequest()
        {
            ValidateReturnUrl(Context.Request.QueryString);

            var cookie = Context.Request.Cookies["_culture"];
            if (cookie == null)
            {
                cookie = new HttpCookie("_culture", Culture.NorwegianCode);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }

            if (!string.IsNullOrEmpty(cookie.Value))
            {
                var culture = new CultureInfo(cookie.Value);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }
        protected void Application_PreSendRequestHeaders()
        {
             Response.Headers.Remove("Access-Control-Allow-Origin");
        }

        protected void Application_EndRequest()
        {
            try
            {
                var redirectUri = HttpContext.Current.Request.Url.AbsoluteUri;

                var loggedInCookie = Context.Request.Cookies["_loggedIn"];
                if (string.IsNullOrEmpty(Request.QueryString["autologin"]) && loggedInCookie != null && loggedInCookie.Value == "true" && !Request.IsAuthenticated)
                {
                    if (Request.Path != "/Home/SignOut" && Request.Path != "/signout-callback-oidc" && Request.QueryString["logout"] != "true" && Request.Path != "/shared-partials-scripts" && Request.Path != "/shared-partials-styles" && !Request.Path.Contains("Content") && Request.Path != "/Content/local-styles" && Request.Path != "/Content/bower_components/kartverket-felleskomponenter/assets/js/scripts" && Request.Path != "/Scripts/local-scripts" && !Request.Path.Contains("node_modules") && !Request.Path.Contains("dist"))
                        Response.Redirect("/Home/SignIn?autologin=true&ReturnUrl=" + redirectUri);
                }
            }

            catch (Exception ex)
            {
            }
        }


        void ValidateReturnUrl(NameValueCollection queryString)
        {
            if (queryString != null)
            {
                var returnUrl = queryString.Get("returnUrl");
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = returnUrl.Replace("http://", "");
                    returnUrl = returnUrl.Replace("https://", "");

                    var host = Request.Url.Host;
                    if (returnUrl.StartsWith("localhost:44350"))
                        host = "localhost";

                    if (!returnUrl.StartsWith(host))
                        HttpContext.Current.Response.StatusCode = 400;
                }
            }
        }
    }
}
