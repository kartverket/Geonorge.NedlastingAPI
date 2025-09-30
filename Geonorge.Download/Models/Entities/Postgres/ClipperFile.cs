using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Geonorge.Download.Models.Postgres
{
    public class ClipperFile
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime DateUploaded { get; set; }
        public string? File { get; set; }
        public bool Valid { get; set; }
        public string? Message { get; set; }
    }
}