using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WebApi.BasicAuth;

namespace Kartverket.Geonorge.Download
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            config.MapHttpAttributeRoutes();

            config.EnableBasicAuth();

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new DefaultContractResolver();
            config.Formatters.XmlFormatter.UseXmlSerializer = true;
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};

            // don't show exception stacktrace to the public
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;

            config.MessageHandlers.Add(new MessageLoggingHandler());

            config.EnsureInitialized();
        }
    }
}