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
            filliste = new HashSet<filliste>();
        }
        [Key]
        public int ID { get; set; }

        [StringLength(255)]
        public string Tittel { get; set; }

        [StringLength(255)]
        public string metadataUuid { get; set; }

        public bool? supportsAreaSelection { get; set; }

        public bool? supportsFormatSelection { get; set; }

        public bool? supportsPolygonSelection { get; set; }

        public bool? supportsProjectionSelection { get; set; }

        public virtual ICollection<filliste> filliste { get; set; }
    }
}
