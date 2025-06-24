// TODO: Caching

//using System.Linq;
//using System.Net.Http.Formatting;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Web.Http;
//using System.Web.Http.Controllers;
//using WebApi.OutputCache.V2;

//namespace Geonorge.Download.Services
//{
//    public class GeonorgeCacheOutputAttribute : CacheOutputAttribute
//    {

//        protected override MediaTypeHeaderValue GetExpectedMediaType(HttpConfiguration config, HttpActionContext actionContext)
//        {
//            if (!string.IsNullOrEmpty(MediaType))
//            {
//                return new MediaTypeHeaderValue(MediaType);
//            }

//            MediaTypeHeaderValue responseMediaType = null;
//            if (actionContext.Request.Headers.Accept != null)
//            {
//                foreach(var header in actionContext.Request.Headers.Accept) { 
//                responseMediaType = header;
//                    if (responseMediaType != null && config.Formatters.Any(x => x.SupportedMediaTypes.Any(value => value.MediaType == responseMediaType.MediaType)))
//                    {
//                        return new MediaTypeHeaderValue(responseMediaType.MediaType);
//                    }
//                }
//            }

//            var negotiator = config.Services.GetService(typeof(IContentNegotiator)) as IContentNegotiator;
//            var returnType = actionContext.ActionDescriptor.ReturnType;

            
//            var negotiatedResult = negotiator.Negotiate(returnType, actionContext.Request, config.Formatters);

//            if (negotiatedResult == null)
//            {
//                return DefaultMediaType;
//            }

//            responseMediaType = negotiatedResult.MediaType;
//            if (string.IsNullOrWhiteSpace(responseMediaType.CharSet))
//            {
//                responseMediaType.CharSet = Encoding.UTF8.HeaderName;
//            }
            

//            return responseMediaType;
//        }

//    }
//}