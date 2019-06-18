using Autofac;
using Geonorge.AuthLib.NetFull;
using Kartverket.Geonorge.Download.App_Start;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Kartverket.Geonorge.Download.Startup))]

namespace Kartverket.Geonorge.Download
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Use Autofac as an Owin middleware
            var container = DependencyConfig.Configure(new ContainerBuilder());
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();  // requires Autofac.Mvc5.Owin nuget package installed
            
            app.UseGeonorgeAuthentication();
        }
    }
}
