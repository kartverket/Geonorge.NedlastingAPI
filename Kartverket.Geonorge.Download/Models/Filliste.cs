namespace Kartverket.Geonorge.Download.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("filliste")]
    public partial class filliste
    {
        [Key]
        public Guid? id { get; set; }

       
        [Column(Order = 0)]
        [StringLength(100)]
        public string filnavn { get; set; }

        [Column(Order = 1, TypeName = "ntext")]
        public string url { get; set; }

        [StringLength(50)]
        public string kategori { get; set; }

        [StringLength(50)]
        public string underkategori { get; set; }

        [StringLength(50)]
        public string inndeling { get; set; }

        [StringLength(100)]
        public string inndelingsverdi { get; set; }

        [StringLength(100)]
        public string projeksjon { get; set; }

        [StringLength(100)]
        public string format { get; set; }

        public int? dataset { get; set; }
        [ForeignKey("dataset")]
        public virtual Dataset Dataset1 { get; set; }
    }
}
