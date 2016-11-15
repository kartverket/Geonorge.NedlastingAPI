using Kartverket.Geonorge.Download.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Geonorge.NedlastingApi.V1;

namespace Kartverket.Geonorge.Download.Services
{
    public class CapabilitiesService : ICapabilitiesService
    {
        DownloadContext db = new DownloadContext();
        RegisterFetcher register = new RegisterFetcher();

        public CapabilitiesService() 
        {
        }

        public CapabilitiesType GetCapabilities(string metadataUuid, string apiVersionId) 
        {
            var dataset = GetDataset(metadataUuid);

            if (dataset == null) return null;

            return new CapabilitiesType
            {
                supportsAreaSelection = dataset.supportsAreaSelection,
                supportsFormatSelection = dataset.supportsFormatSelection,
                supportsPolygonSelection = dataset.supportsPolygonSelection,
                supportsProjectionSelection = dataset.supportsProjectionSelection,
                mapSelectionLayer = dataset.mapSelectionLayer,
                _links = new CapabilityLinksCreator().CreateCapabilityLinks(metadataUuid, apiVersionId).ToArray()
            };
        }

        public Dataset GetDataset(string metadataUuid)
        {
            return (from c in db.Capabilities
                where c.metadataUuid == metadataUuid
                select c).FirstOrDefault();
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