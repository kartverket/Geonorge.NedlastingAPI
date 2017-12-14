using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Geonorge.NedlastingApi.V3;

namespace Kartverket.Geonorge.Download.Models
{
    public class LinkCreator
    {
        private const string RelSelf = "self";
        private const string RelCapabilities = "http://rel.geonorge.no/download/capabilities";
        private const string RelProjection = "http://rel.geonorge.no/download/projection";
        private const string RelFormat = "http://rel.geonorge.no/download/format";
        private const string RelArea = "http://rel.geonorge.no/download/area";
        private const string RelOrder = "http://rel.geonorge.no/download/order";
        private const string RelCanDownload = "http://rel.geonorge.no/download/can-download";
        private const string RelOrderBundle = "http://rel.geonorge.no/download/order/bundle";
        

        public string GetDefaultApiVersion()
        {
            return "3";
        }

        public List<LinkType> CreateCapabilityLinks(string metadataUuid)
        {
            var links = CreateLinks(metadataUuid);

            var capabilityLink = links.FirstOrDefault(l => l.rel == RelCapabilities);
            if (capabilityLink != null)
                capabilityLink.rel = RelSelf;

            return links;
        }


        public List<LinkType> CreateLinks()
        {
            return CreateLinks(null);
        }

        private List<LinkType> CreateLinks(string metadataUuid)
        {
            var apiBaseUrl = GetApiBaseUrl();

            var links = new List<LinkType>
            {
                CreateProjectionLink(metadataUuid, apiBaseUrl),
                CreateFormatLink(metadataUuid, apiBaseUrl),
                CreateAreaLink(metadataUuid, apiBaseUrl),
                CreateOrderLink(apiBaseUrl),
                CreateCapabilitiesLink(metadataUuid, apiBaseUrl),
                CreateCanDownloadLink(apiBaseUrl)
            };
            return links;
        }

        public LinkType[] CreateOrderReceiptLinks(Guid orderUuid)
        {
            var apiBaseUrl = GetApiBaseUrl();

            var links = new List<LinkType>
            {
                CreateOrderLink(apiBaseUrl, orderUuid, true),
                CreateOrderBundleLink(apiBaseUrl, orderUuid)
            };
            return links.ToArray();
        }

        private LinkType CreateOrderBundleLink(string apiBaseUrl, Guid orderUuid)
        {
            LinkType link = CreateOrderLink(apiBaseUrl, orderUuid);
            link.rel = RelOrderBundle;
            return link;
        }

        private LinkType CreateCanDownloadLink(string apiBaseUrl)
        {
            return new LinkType
            {
                rel = RelCanDownload,
                href = apiBaseUrl + "can-download"
            };
        }

        private static LinkType CreateOrderLink(string apiBaseUrl, Guid? orderUuid = null, bool isSelf=false)
        {
            string href = apiBaseUrl + "order";

            if (orderUuid.HasValue)
                href = href + "/" + orderUuid.Value;

            string rel = RelOrder;
            if (isSelf)
                rel = RelSelf;

            return new LinkType
            {
                rel = rel,
                href = href
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