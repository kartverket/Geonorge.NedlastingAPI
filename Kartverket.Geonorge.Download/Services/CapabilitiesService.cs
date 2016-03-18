using Kartverket.Geonorge.Download.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace Kartverket.Geonorge.Download.Services
{
    public class CapabilitiesService
    {
        DownloadContext db = new DownloadContext();
        RegisterFetcher register = new RegisterFetcher();

        public CapabilitiesService() 
        {
        }

        public CapabilitiesType GetCapabilities(string metadataUuid) 
        {

            var capabilitiesQuery = from c in db.Capabilities
                                    where c.metadataUuid == metadataUuid
                                    select c;

            var capability = capabilitiesQuery.FirstOrDefault();
            CapabilitiesType capabilities = new CapabilitiesType();
            capabilities.supportsAreaSelection = capability.supportsAreaSelection;
            capabilities.supportsFormatSelection = capability.supportsFormatSelection;
            capabilities.supportsPolygonSelection = capability.supportsPolygonSelection;
            capabilities.supportsProjectionSelection = capability.supportsProjectionSelection;
            capabilities.mapSelectionLayer = capability.mapSelectionLayer;

            List<LinkType> links = new List<LinkType>();

            LinkType l1 = new LinkType();
            l1.rel = "http://rel.geonorge.no/download/projection";
            l1.href = WebConfigurationManager.AppSettings["DownloadUrl"] + "api/codelists/projection/" + metadataUuid;

            LinkType l2 = new LinkType();
            l2.rel = "http://rel.geonorge.no/download/format";
            l2.href = WebConfigurationManager.AppSettings["DownloadUrl"] + "api/codelists/format/" + metadataUuid;

            LinkType l3 = new LinkType();
            l3.rel = "http://rel.geonorge.no/download/area";
            l3.href = WebConfigurationManager.AppSettings["DownloadUrl"] + "api/codelists/area/" + metadataUuid;

            LinkType l4 = new LinkType();
            l4.rel = "http://rel.geonorge.no/download/order";
            l4.href = WebConfigurationManager.AppSettings["DownloadUrl"] + "api/order";

            links.Add(l1); links.Add(l2); links.Add(l3); links.Add(l4);

            capabilities._links = links.ToArray();


            return capabilities;
        }


        public List<ProjectionType> GetProjections(string metadataUuid)
        {

            var projectionsQuery = (from p in db.FileList
                                   where p.Dataset1.metadataUuid == metadataUuid
                                   select p.projeksjon).Distinct();

            List<ProjectionType> projections = new List<ProjectionType>();

            foreach (var projection in projectionsQuery.ToList())
            {

                ProjectionType p1 = new ProjectionType();
                p1.code = projection.ToString();
                p1.codespace = register.GetProjection(projection.ToString()).codespace;
                p1.name = register.GetProjection(projection.ToString()).name;

                projections.Add(p1);
            }

            return projections;
        }


        public List<AreaType> GetAreas(string metadataUuid)
        {
            var areasQuery = (from p in db.FileList
                              where p.Dataset1.metadataUuid == metadataUuid
                              select new { p.inndeling, p.inndelingsverdi, p.projeksjon, p.format }).Distinct().ToList() ;

            List<AreaType> areas = new List<AreaType>();

            foreach (var area in areasQuery.Select(a => new { a.inndeling, a.inndelingsverdi }).Distinct() )
            { 
            AreaType a1 = new AreaType();
            a1.type = area.inndeling;
            a1.code = area.inndelingsverdi;
            a1.name = register.GetArea(a1.type, a1.code).name;
            areas.Add(a1);
            }

            areas = areas.OrderBy(o => o.type).ThenBy(n => n.name).ToList();

            for (int i = 0; i < areas.Count(); i++)
            {

                string type = areas[i].type;
                string code = areas[i].code;


                List<ProjectionType> projections = new List<ProjectionType>();

                foreach (var data in areasQuery.Where(p => p.inndeling == type && p.inndelingsverdi == code).Select(a => new { a.projeksjon }).Distinct())
                {
                    projections.Add(new ProjectionType
                    {
                        code = data.projeksjon,
                        codespace = register.GetProjection(data.projeksjon).codespace,
                        name = register.GetProjection(data.projeksjon).name
                    });
                }

                areas[i].projections = projections.ToArray();

                List<FormatType> formats = new List<FormatType>();


                foreach (var data in areasQuery.Where(p => p.inndeling == type && p.inndelingsverdi == code).Select(a => new { a.format }).Distinct())
                {
                    formats.Add(new FormatType { name = data.format });
                }

                areas[i].formats = formats.ToArray();

            }

            return areas;
        }


        public List<FormatType> GetFormats(string metadataUuid)
        {
            var formatsQuery = (from p in db.FileList
                              where p.Dataset1.metadataUuid == metadataUuid
                              select p.format).Distinct();

            List<FormatType> formats = new List<FormatType>();

            foreach (var format in formatsQuery)
            {
            FormatType f1 = new FormatType();
            f1.name = format.ToString();

            formats.Add(f1);
            }


            return formats;
        }
        



    }
}