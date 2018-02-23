using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kartverket.Geonorge.Download.Models
{
    [Table("Dataset")]
    public class Dataset
    {
        [Key] [Column("ID")] 
        public int Id { get; set; }

        [Column("Tittel")] 
        [StringLength(255)] 
        public string Title { get; set; }

        [Column("metadataUuid")]
        [StringLength(255)]
        public string MetadataUuid { get; set; }

        [Column("supportsAreaSelection")] 
        public bool? SupportsAreaSelection { get; set; }

        [Column("supportsFormatSelection")] 
        public bool? SupportsFormatSelection { get; set; }

        [Column("supportsPolygonSelection")] 
        public bool? SupportsPolygonSelection { get; set; }

        [Column("supportsProjectionSelection")]
        public bool? SupportsProjectionSelection { get; set; }

        [Column("fmeklippeUrl")] 
        public string FmeClippingUrl { get; set; }

        [Column("mapSelectionLayer")] 
        public string MapSelectionLayer { get; set; }

        [Column("AccessConstraint")] 
        public string AccessConstraint { get; set; }

        [Column("maxArea")] 
        public int MaxArea { get; set; }

        public virtual ICollection<File> Files { get; set; }

        public Dataset()
        {
            Files = new HashSet<File>();
        }

        public bool IsRestricted()
        {
            return !string.IsNullOrWhiteSpace(AccessConstraint);
        }
    }
}