using System;
using System.Linq;
using Geonorge.NedlastingApi.V3;
using Geonorge.Download.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Geonorge.Download.Controllers.Api
{
    //public class ApiDescriptionController(IConfiguration config) : ControllerBase
    //{
    //    [HttpGet]
    //    [Route("api")]
    //    [ApiExplorerSettings(IgnoreApi = true)]
    //    public IActionResult Index()
    //    {
    //        return Ok(ApiDescription(config, Request));
    //    }

    //    [HttpGet]
    //    [Route("api/capabilities")]
    //    [ApiExplorerSettings(IgnoreApi = true)]
    //    public IActionResult CapabilitiesIndex()
    //    {
    //        if (HtmlIsFirstElementOfAcceptHeader())
    //            return Redirect(UrlToHelpPages());

    //        return Ok(ApiDescription(config, Request));
    //    }

    //    private string UrlToHelpPages()
    //    {
    //        return $"{Request.Scheme}://{Request.Host}/Help/Api/GET-api-capabilities-metadataUuid";
    //    }

    //    private bool HtmlIsFirstElementOfAcceptHeader()
    //    {
    //        var acceptHeader = Request.Headers[HeaderNames.Accept].ToString();
    //        return acceptHeader.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
    //    }

    //    private static VersionResponseType ApiDescription(IConfiguration config, HttpRequest request)
    //    {
    //        var capabilityLinksCreator = new LinkCreator();
    //        var apiVersionDescription = new VersionResponseType
    //        {
    //            version = capabilityLinksCreator.GetDefaultApiVersion(),
    //            _links = capabilityLinksCreator.CreateLinks(config, request).ToArray()
    //        };
    //        return apiVersionDescription;
    //    }
    //}
}