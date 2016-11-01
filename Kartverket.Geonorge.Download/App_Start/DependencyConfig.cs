using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Services;

namespace Kartverket.Geonorge.Download.App_Start
{
    public static class DependencyConfig
    {
        public static void Configure(ContainerBuilder builder)
        {
            ConfigureInfrastructure(builder);

            ConfigureApplicationDependencies(builder);

            var container = builder.Build();

            SetupAspMvcDependencyResolver(container);
        }

        private static void ConfigureApplicationDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<DownloadContext>().InstancePerRequest().AsSelf();
            builder.RegisterType<OrderServiceV2>().As<IOrderService>();
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
            builder.RegisterControllers(typeof(WebApiApplication).Assembly).PropertiesAutowired();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();
            builder.RegisterModule(new AutofacWebTypesModule());
        }
    }
}