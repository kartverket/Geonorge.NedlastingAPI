using System;
using System.Linq;
using System.Net.Mime;
using System.Web.Http;
using System.Web.Http.Description;
using Geonorge.NedlastingApi.V3;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Controllers.Api
{
    public class ApiDescriptionController : ApiController
    {
        [HttpGet]
        [Route("api")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult Index()
        {
            return Ok(ApiDescription());
        }

        [HttpGet]
        [Route("api/capabilities")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult CapabilitiesIndex()
        {
            if (HtmlIsFirstElementOfAcceptHeader())
                return Redirect(UrlToHelpPages());

            return Ok(ApiDescription());
        }

        private string UrlToHelpPages()
        {
            return Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/Help/Api/GET-api-capabilities-metadataUuid";
        }

        private bool HtmlIsFirstElementOfAcceptHeader()
        {
            return Request.Headers.Accept.First().MediaType.Equals(MediaTypeNames.Text.Html);
        }

        private static VersionResponseType ApiDescription()
        {
            var capabilityLinksCreator = new LinkCreator();
            var apiVersionDescription = new VersionResponseType
            {
                version = capabilityLinksCreator.GetDefaultApiVersion(),
                _links = capabilityLinksCreator.CreateLinks().ToArray()
            };
            return apiVersionDescription;
        }
    }
}