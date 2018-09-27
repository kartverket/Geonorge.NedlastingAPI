using GeoNorgeAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class UpdateMetadataInformation
    {
        public string Uuid { get; set; }
        public List<SimpleDistribution> Distributions { get; set; }
        public List<SimpleReferenceSystem> Projections { get; set; }
        public string CoverageLayer { get; set; }

    }
}