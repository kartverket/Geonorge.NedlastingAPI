using System;
using System.Collections.Generic;

namespace Geonorge.Download.Models
{
    public class DatasetAccessConstraint
    {
       
        public string MetadataUuid { get; set; }
        public AccessConstraint AccessConstraint { get; set; }
        public List<FileAccessConstraint> FileAccessConstraints { get; set; }
    }
}