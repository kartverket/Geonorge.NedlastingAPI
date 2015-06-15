using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kartverket.Geonorge.Download.Models
{
    public class CapabilitiesModel
    {

        public CapabilitiesModel() 
        {
            Capabilities = new CapabilitiesType();
  
        }
        public CapabilitiesType Capabilities { get; set; }

    }
}
