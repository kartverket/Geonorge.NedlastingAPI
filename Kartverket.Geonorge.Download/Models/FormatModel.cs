using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class FormatModel
    {
        public FormatModel() 
        {
            Formats = new List<FormatType>();
  
        }
        public List<FormatType> Formats { get; set; }
    }
}