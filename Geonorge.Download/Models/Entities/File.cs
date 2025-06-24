using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Geonorge.Download.Models
{
    [Table("filliste")]
    public class File
    {
        [Key] [Column("id")] 
        public Guid? Id { get; set; }

        [Column("filnavn", Order = 0)]
        [StringLength(100)]
        public string? Filename { get; set; }

        [Column("url", Order = 1, TypeName = "ntext")]
        public string? Url { get; set; }

        [Column("kategori")]
        [StringLength(50)]
        public string? Category { get; set; }

        [Column("underkategori")]
        [StringLength(50)]
        public string? SubCategory { get; set; }

        [Column("inndeling")]
        [StringLength(50)]
        public string? Division { get; set; }

        [Column("inndelingsverdi")]
        [StringLength(100)]
        public string? DivisionKey { get; set; }

        [Column("projeksjon")]
        [StringLength(100)]
        public string? Projection { get; set; }

        [Column("format")] 
        [StringLength(100)] 
        public string? Format { get; set; }

        [Column("dataset")] 
        public int? DatasetId { get; set; }

        [ForeignKey("DatasetId")] 
        public virtual Dataset Dataset { get; set; }

        [Column("AccessConstraintRequiredRole")]
        public string? AccessConstraintRequiredRole { get; set; }
    }
}