using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Geonorge.NedlastingApi.V3;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class CapabilityLinksCreator
    {
        private const string RelCapabilities = "http://rel.geonorge.no/download/capabilities";
        private const string RelProjection = "http://rel.geonorge.no/download/projection";
        private const string RelFormat = "http://rel.geonorge.no/download/format";
        private const string RelArea = "http://rel.geonorge.no/download/area";
        private const string RelOrder = "http://rel.geonorge.no/download/order";
        private const string RelCanDownload = "http://rel.geonorge.no/download/can-download";

        public string GetDefaultApiVersion()
        {
            return "3";
        }

        public List<LinkType> CreateCapabilityLinks(string metadataUuid)
        {
            List<LinkType> links = CreateLinks(metadataUuid);

            var capabilityLink = links.FirstOrDefault(l => l.rel == RelCapabilities);
            if (capabilityLink != null)
                capabilityLink.rel = "self";

            return links;
        }


        public List<LinkType> CreateLinks()
        {
            return CreateLinks(null);
        }

        public List<LinkType> CreateLinks(string metadataUuid)
        {
            var apiBaseUrl = GetApiBaseUrl();

            var links = new List<LinkType>();
            links.Add(CreateProjectionLink(metadataUuid, apiBaseUrl));
            links.Add(CreateFormatLink(metadataUuid, apiBaseUrl));
            links.Add(CreateAreaLink(metadataUuid, apiBaseUrl));
            links.Add(CreateOrderLink(apiBaseUrl));
            links.Add(CreateCapabilitiesLink(metadataUuid, apiBaseUrl));
            links.Add(CreateCanDownloadLink(apiBaseUrl));
            return links;
        }

        private LinkType CreateCanDownloadLink(string apiBaseUrl)
        {
            return new LinkType
            {
                rel = RelCanDownload,
                href = apiBaseUrl + "can-download"
            };
        }

        private static LinkType CreateOrderLink(string apiBaseUrl)
        {
            return new LinkType
            {
                rel = RelOrder,
                href = apiBaseUrl + "order"
            };
        }

        private static LinkType CreateCapabilitiesLink(string metadataUuid, string apiBaseUrl)
        {
            return new LinkType
            {
                rel = RelCapabilities,
                href = GetCapabilitiesUrl(metadataUuid, apiBaseUrl)
            };
        }

       

        private static LinkType CreateAreaLink(string metadataUuid, string apiBaseUrl)
        {
            return new LinkType
            {
                rel = RelArea,
                href = GetAreaUrl(metadataUuid, apiBaseUrl)
            };
        }

        private static LinkType CreateFormatLink(string metadataUuid, string apiBaseUrl)
        {
            return new LinkType
            {
                rel = RelFormat,
                href = GetFormatUrl(metadataUuid, apiBaseUrl)
            };
        }

        private static LinkType CreateProjectionLink(string metadataUuid, string apiBaseUrl)
        {
            return new LinkType
            {
                rel = RelProjection,
                href = GetProjectionUrl(metadataUuid, apiBaseUrl)
            };
        }
        private static string GetCapabilitiesUrl(string metadataUuid, string apiBaseUrl)
        {
            var currentUrl = HttpContext.Current.Request.Url.AbsoluteUri;
            if (currentUrl.Contains("capabilities"))
                return currentUrl;
            else
                return GetTemplatedUrlIfEmptyParam(apiBaseUrl + "capabilities/", metadataUuid);
        }

        private static string GetAreaUrl(string metadataUuid, string apiBaseUrl)
        {
            return GetTemplatedUrlIfEmptyParam(apiBaseUrl + "codelists/area/", metadataUuid);
        }

        private static string GetFormatUrl(string metadataUuid, string apiBaseUrl)
        {
            return GetTemplatedUrlIfEmptyParam(apiBaseUrl + "codelists/format/", metadataUuid);
        }

        private static string GetProjectionUrl(string metadataUuid, string apiBaseUrl)
        {
            return GetTemplatedUrlIfEmptyParam(apiBaseUrl + "codelists/projection/", metadataUuid);
        }

        private static string GetTemplatedUrlIfEmptyParam(string baseUrl, string metadataUuid)
        {
            if (string.IsNullOrWhiteSpace(metadataUuid))
                return baseUrl + "{metadataUuid}";

            return baseUrl + metadataUuid;
        }

        private static string GetApiBaseUrl()
        {
            return WebConfigurationManager.AppSettings["DownloadUrl"] + "api/";
        }
    }
}