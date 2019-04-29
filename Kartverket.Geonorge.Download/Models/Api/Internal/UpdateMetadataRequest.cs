using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models.Api.Internal
{
    public class UpdateMetadataRequest
    {
        /// <summary>
        ///     The metadata uuid to update
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        ///     The list of distributions
        /// </summary>
        public List<Distribution> Distributions { get; set; }
        /// <summary>
        ///     The list of projections
        /// </summary>
        public List<Projection> Projections { get; set; }
        /// <summary>
        ///     The coverage layer to display coverage in map
        /// </summary>
        public string CoverageLayer { get; set; }
        /// <summary>
        ///     The date when the dataset was updated
        /// </summary>
        public DateTime? DatasetDateUpdated { get; set; }
    }

    public class Distribution
    {
        /// <summary>
        ///     URL for getting dataset info, default currently https://nedlasting.geonorge.no/api/capabilities/
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        ///     Should be set to: GEONORGE:DOWNLOAD (default)
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        ///     Organisation for distribution ex: Kartverket
        /// </summary>
        public string Organization { get; set; }
        /// <summary>
        ///     Possible values: fylkesvis, landsfiler, kartbladvis, kommunevis, regional inndeling
        /// </summary>
        public string UnitsOfDistribution { get; set; }
        /// <summary>
        ///     The list of formats
        /// </summary>
        public List<Format> Formats { get; set; }
    }

    public class Format
    {
        /// <summary>
        ///     Name of format ex. : SOSI
        /// </summary>
        public string FormatName { get; set; }
        /// <summary>
        ///     Version of format ex. : 4.5
        /// </summary>
        public string FormatVersion { get; set; }
    }
    public class Projection
    {
        /// <summary>
        ///     Projection EPSG code ex: 23032
        /// </summary>
        public string EPSGCode { get; set; }
    }
}