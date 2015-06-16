using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Kartverket.Geonorge.Download.Models
{

    public partial class CapabilitiesModel
    {

        public CapabilitiesModel() 
        {
            Capabilities = new CapabilitiesType();
  
        }
        public CapabilitiesType Capabilities { get; set; }
        [IgnoreDataMember]
        public string metadataUUid { get; set; }
        [IgnoreDataMember]
        public int ID { get; set; }

    }
}
