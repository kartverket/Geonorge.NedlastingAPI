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

            DependencyConfig.Configure(new ContainerBuilder());


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

        protected void Application_BeginRequest()
        {
            DeleteOldPeronalData();

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

        protected void DeleteOldPeronalData()
        {
            //Remove personal info older than 1 day
            var _context = DependencyResolver.Current.GetService<DownloadContext>();
            _context.Database.ExecuteSqlCommand("UPDATE [kartverket_nedlasting].[dbo].[orderDownload] set email = '', username = '' where orderDate < DATEADD(day, -1, GETDATE())");
        }
    }
}
