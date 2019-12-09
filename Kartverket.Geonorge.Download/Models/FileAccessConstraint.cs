using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class FileAccessConstraint
    {
        public string MetadataUuid { get; set; }
        public string File { get; set; }
        public string Role { get; set; }
        public List<string> Roles { get; set; }
    }
}