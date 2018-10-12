using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class OrderUsage
    {
        public List<string> Uuids { get; set; } //Finnes det alltid Uuid massiv nedlasting? Skal vi ha med projeksjon,format område?
        public string Group { get; set; }  //http://register.dev.geonorge.no/metadata-kodelister/brukergrupper
        public List<string> Purposes { get; set; } //http://register.dev.geonorge.no/metadata-kodelister/formal
    }
}