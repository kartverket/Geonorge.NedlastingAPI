using System.Collections.Generic;
using System.Web.Configuration;
using Geonorge.NedlastingApi.V1;

namespace Kartverket.Geonorge.Download.Models
{
    public class CapabilityLinksCreator
    {
        public List<LinkType> CreateCapabilityLinks(string metadataUuid, string versionId)
        {
            var apiBaseUrl = GetApiBaseUrl(versionId);

            var links = new List<LinkType>();
            links.Add(new LinkType
            {
                rel = "http://rel.geonorge.no/download/projection",
                href = apiBaseUrl + "codelists/projection/" + metadataUuid
            });

            links.Add(new LinkType
            {
                rel = "http://rel.geonorge.no/download/format",
                href = apiBaseUrl + "codelists/format/" + metadataUuid
            });

            links.Add(new LinkType
            {
                rel = "http://rel.geonorge.no/download/area",
                href = apiBaseUrl + "codelists/area/" + metadataUuid
            });

            links.Add(new LinkType
            {
                rel = "http://rel.geonorge.no/download/order",
                href = apiBaseUrl + "order"
            });

            return links;
        }

        private static string GetApiBaseUrl(string versionId)
        {
            var applicationUrl = WebConfigurationManager.AppSettings["DownloadUrl"];
            string apiBaseUrl;

            if (versionId == "v1")
            {
                apiBaseUrl = applicationUrl + "api/";
            }
            else
            {
                apiBaseUrl = applicationUrl + "api/" + versionId + "/";
            }
            return apiBaseUrl;
        }
    }
}