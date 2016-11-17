using System;

namespace Kartverket.Geonorge.Download.Models
{
    public class DatasetAccessConstraint
    {
       
        public string MetadataUuid { get; set; }
        public AccessConstraint AccessConstraint { get; set; }
    }
}