using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class ProjectionsModel
    {
        public ProjectionsModel() 
        {
            Projections = new List<ProjectionType>();
  
        }
        public List<ProjectionType> Projections { get; set; }

    }
}
