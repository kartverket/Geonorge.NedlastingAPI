using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GeoNorgeAPI;
using Kartverket.Geonorge.Download.Models;
using Kartverket.Geonorge.Download.Models.Api.Internal;
using www.opengis.net;

namespace Kartverket.Geonorge.Download.Services
{
    public class UpdateMetadataService : IUpdateMetadataService
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UpdateMetadataInformation Convert(UpdateMetadataRequest metadata)
        {
            UpdateMetadataInformation information = new UpdateMetadataInformation();
            information.Uuid = metadata.Uuid;
            information.Distributions = new List<SimpleDistribution>();
            information.Projections = new List<SimpleReferenceSystem>();
            information.CoverageLayer = metadata.CoverageLayer;

            foreach(var distribution in metadata.Distributions)
            {
                foreach (var format in distribution.Formats)
                {
                    information.Distributions.Add( new SimpleDistribution
                    {
                        URL = distribution.URL,
                        Protocol = distribution.Protocol,
                        Organization = distribution.Organization,
                        UnitsOfDistribution = distribution.UnitsOfDistribution,
                        FormatName = format.FormatName,
                        FormatVersion = format.FormatVersion
                    }
                    );
                }
            }


            foreach (var projection in metadata.Projections)
            {
                information.Projections.Add( new SimpleReferenceSystem
                {
                    Namespace = "EPSG", CoordinateSystem = "http://www.opengis.net/def/crs/EPSG/0/" + projection.EPSGCode  }
                );

            }

            return information;
        }

        public void UpdateMetadata(UpdateMetadataInformation metadataInfo)
        {
            System.Collections.Specialized.NameValueCollection settings = System.Web.Configuration.WebConfigurationManager.AppSettings;
            string server = settings["GeoNetworkUrl"];
            string usernameGeonetwork = settings["GeoNetworkUsername"];
            string password = settings["GeoNetworkPassword"];
            string geonorgeUsername = settings["GeonorgeUsername"];


            GeoNorge api = new GeoNorge(usernameGeonetwork, password, server);
            api.OnLogEventDebug += new GeoNorgeAPI.LogEventHandlerDebug(LogEventsDebug);
            api.OnLogEventError += new GeoNorgeAPI.LogEventHandlerError(LogEventsError);

            var metadata = api.GetRecordByUuid(metadataInfo.Uuid);

            SimpleMetadata simpleMetadata = new SimpleMetadata(metadata);

            List<SimpleDistribution> distributionFormats = simpleMetadata.DistributionsFormats;

            List<SimpleDistribution> distributionFormatsUpdated = new List<SimpleDistribution>();

            if (HasGeonorgeDownload(metadataInfo.Distributions))
            {

                foreach (var distribution in distributionFormats)
                {
                    if (distribution.Protocol != "GEONORGE:DOWNLOAD")
                    {
                        distributionFormatsUpdated.Add(distribution);
                    }
                }

            }
            else
            {
                distributionFormatsUpdated = distributionFormats;
            }

            distributionFormatsUpdated.InsertRange(0, metadataInfo.Distributions);

            simpleMetadata.DistributionsFormats = distributionFormatsUpdated;
            simpleMetadata.DistributionDetails = new SimpleDistributionDetails
            {
                URL = distributionFormatsUpdated[0].URL,
                Protocol = distributionFormatsUpdated[0].Protocol,
                UnitsOfDistribution = distributionFormatsUpdated[0].UnitsOfDistribution
            };

            simpleMetadata.ReferenceSystems = metadataInfo.Projections;

            if(!string.IsNullOrEmpty(metadataInfo.CoverageLayer))
                simpleMetadata.CoverageUrl = "TYPE:GEONORGE-WMS@PATH:https://wms.geonorge.no/wms?@LAYER:" + metadataInfo.CoverageLayer;

            simpleMetadata.DateMetadataUpdated = DateTime.Now;

            api.MetadataUpdate(simpleMetadata.GetMetadata(), CreateAdditionalHeadersWithUsername(geonorgeUsername, "true"));
            Log.Info($"Metadata updated for uuid: {metadataInfo.Uuid}");
        }

        private bool HasGeonorgeDownload(List<SimpleDistribution> distributions)
        {
            for (int i = 0; i < distributions.Count; i++)
            {
                if (distributions[i].Protocol == "GEONORGE:DOWNLOAD")
                {
                    return true;
                }
            }
            return false;
        }

        private void LogEventsDebug(string log)
        {

            Log.Debug(log);
        }

        private void LogEventsError(string log, Exception ex)
        {
            Log.Error(log, ex);
        }

        public Dictionary<string, string> CreateAdditionalHeadersWithUsername(string username, string published = "")
        {
            Dictionary<string, string> header = new Dictionary<string, string> { { "GeonorgeUsername", username } };

             header.Add("GeonorgeOrganization", "Kartverket");
             header.Add("GeonorgeRole", "nd.metadata_admin");
             header.Add("published", published);

            return header;
        }
    }
}