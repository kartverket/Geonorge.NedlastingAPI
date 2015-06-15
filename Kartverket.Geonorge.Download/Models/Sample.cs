using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class Sample
    {
        public Sample() 
        {
 
        }

        public CapabilitiesModel GetCapabilities(string metadataUuid)
        {

            CapabilitiesModel cap = new CapabilitiesModel();

                cap.Capabilities.supportsAreaSelection = false;
                cap.Capabilities.supportsFormatSelection = true;
                cap.Capabilities.supportsPolygonSelection = false;
                cap.Capabilities.supportsProjectionSelection = true;


            return cap;
        
        }


        public ProjectionsModel GetProjections(string metadataUuid)
        {

            ProjectionsModel m = new ProjectionsModel();
   
                ProjectionType p1 = new ProjectionType();
                p1.code = "http://www.opengis.net/def/crs/EPSG/0/25830";
                p1.codespace = "EPSG";
                p1.name = "UTM sone 30, EUREF89 (ETRS89/UTM), 2d";

                ProjectionType p2 = new ProjectionType();
                p2.code = "http://www.opengis.net/def/crs/EPSG/0/4258";
                p2.codespace = "EPSG";
                p2.name = "EUREF 89 Geografisk (ETRS 89) 2d";

                m.Projections.Add(p1);
                m.Projections.Add(p2);
            

            return m;

        }

        public AreasModel GetAreas(string metadataUuid)
        {

            AreasModel m = new AreasModel();

            AreaType a1 = new AreaType();
            a1.type = "kommunevis";
            a1.name = "Notodden";

            AreaType a2 = new AreaType();
            a2.type = "kommunevis";
            a2.name = "Bø i Telemark";

            m.Areas.Add(a1);
            m.Areas.Add(a2);

            return m;

        }


        public FormatModel GetFormats(string metadataUuid)
        {

            FormatModel m = new FormatModel();

            FormatType f1 = new FormatType();
            f1.name = "SOSI";
            f1.version = "4.0";

            FormatType f2 = new FormatType();
            f2.name = "jpg";
            f2.version = "1";            

            m.Formats.Add(f1);
            m.Formats.Add(f2);


            return m;

        }

    }
}