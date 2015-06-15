using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class AreasModel
    {
        public AreasModel() 
        {
            Areas = new List<AreaType>();
  
        }
        public List<AreaType> Areas { get; set; }
    }
}