namespace Kartverket.Geonorge.Download.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Dataset")]
    public partial class Dataset
    {
        public Dataset()
        {
            filliste = new HashSet<File>();
        }
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("Tittel")]
        [StringLength(255)]
        public string Tittel { get; set; }

        [Column("metadataUuid")]
        [StringLength(255)]
        public string metadataUuid { get; set; }

        [Column("supportsAreaSelection")]
        public bool? supportsAreaSelection { get; set; }

        [Column("supportsFormatSelection")]
        public bool? supportsFormatSelection { get; set; }

        [Column("supportsPolygonSelection")]
        public bool? supportsPolygonSelection { get; set; }

        [Column("supportsProjectionSelection")]
        public bool? supportsProjectionSelection { get; set; }

        [Column("fmeklippeUrl")]
        public string fmeklippeUrl { get; set; }

        [Column("mapSelectionLayer")]
        public string mapSelectionLayer { get; set; }

        [Column("AccessConstraint")]
        public string AccessConstraint { get; set; }

        [Column("maxArea")]
        public int MaxArea { get; set; }

        public virtual ICollection<File> filliste { get; set; }
    }
}
