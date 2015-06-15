using Kartverket.Geonorge.Download.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Services
{
    public class CapabilitiesService
    {

        public CapabilitiesService() 
        {
        }

        public CapabilitiesModel GetCapabilities(string metadataUuid) 
        {
            CapabilitiesModel capabilities = GetSampleCapabilities(metadataUuid);

            return capabilities;
        }


        public ProjectionsModel GetProjections(string metadataUuid)
        {
            ProjectionsModel projections = GetSampleProjections(metadataUuid);

            return projections;
        }


        public AreasModel GetAreas(string metadataUuid)
        {
            AreasModel areas = GetSampleAreas(metadataUuid);

            return areas;
        }

        public FormatModel GetFormats(string metadataUuid)
        {
            FormatModel formats = GetSampleFormats(metadataUuid);

            return formats;
        }


        private CapabilitiesModel GetSampleCapabilities(string metadataUuid)
        {
            CapabilitiesModel capability = new Sample().GetCapabilities(metadataUuid);

            return capability;

        }

        private ProjectionsModel GetSampleProjections(string metadataUuid)
        {
            ProjectionsModel projections = new Sample().GetProjections(metadataUuid);

            return projections;
        }

        private AreasModel GetSampleAreas(string metadataUuid)
        {
            AreasModel areas = new Sample().GetAreas(metadataUuid);

            return areas;

        }

        private FormatModel GetSampleFormats(string metadataUuid)
        {
            FormatModel areas = new Sample().GetFormats(metadataUuid);

            return areas;

        }

    }
}