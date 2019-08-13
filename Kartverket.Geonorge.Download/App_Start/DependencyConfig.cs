using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Geonorge.AuthLib.NetFull;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;
using Kartverket.Geonorge.Download.Services.Auth;

namespace Kartverket.Geonorge.Download.App_Start
{
    public static class DependencyConfig
    {
        public static IContainer Configure(ContainerBuilder builder)
        {
            ConfigureInfrastructure(builder);

            ConfigureApplicationDependencies(builder);

            var container = builder.Build();

            SetupAspMvcDependencyResolver(container);

            return container;
        }

        private static void ConfigureApplicationDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<DownloadContext>().InstancePerRequest().AsSelf();
            builder.RegisterType<OrderService>().As<IOrderService>();
            builder.RegisterType<ClipperService>().As<IClipperService>();
            builder.RegisterType<CapabilitiesService>().As<ICapabilitiesService>();
            builder.RegisterType<DownloadService>().As<IDownloadService>();
            builder.RegisterType<RegisterFetcher>().As<IRegisterFetcher>();
            builder.RegisterType<NotificationService>().As<INotificationService>();
            builder.RegisterType<EmailService>().As<IEmailService>();
            builder.RegisterType<UpdateFileStatusService>().As<IUpdateFileStatusService>();
            builder.RegisterType<OrderBundleService>().As<IOrderBundleService>();
            builder.RegisterType<ExternalRequestService>().As<IExternalRequestService>();
            builder.RegisterType<AuthenticationService>().As<IAuthenticationService>();
            builder.RegisterType<BasicAuthenticationCredentialValidator>()
                .As<IBasicAuthenticationCredentialValidator>();

            builder.Register(ctx => new HttpClient()).Named<HttpClient>("tokenValidation").SingleInstance();
            builder.Register(ctx => new GeoIdAuthentication(ctx.ResolveNamed<HttpClient>("tokenValidation"))).As<IGeoIdAuthenticationService>();

            builder.RegisterType<BasicAuthenticationService>().As<IBasicAuthenticationService>();
            builder.RegisterType<UpdateMetadataService>().As<IUpdateMetadataService>();
            builder.RegisterType<FileService>().As<IFileService>();
        }

        private static void SetupAspMvcDependencyResolver(IContainer container)
        {
            // dependency resolver for MVC
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // dependency resolver for Web API
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static void ConfigureInfrastructure(ContainerBuilder builder)
        {
            builder.RegisterFilterProvider();
            builder.RegisterControllers(typeof(WebApiApplication).Assembly).PropertiesAutowired();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();
            builder.RegisterModule(new AutofacWebTypesModule());
            builder.RegisterModule<GeonorgeAuthenticationModule>();
        }
    }
}