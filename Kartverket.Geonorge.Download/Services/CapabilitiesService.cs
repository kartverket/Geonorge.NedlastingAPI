using Kartverket.Geonorge.Download.Models;
using System.Collections.Generic;
using System.Linq;
using Geonorge.NedlastingApi.V3;

namespace Kartverket.Geonorge.Download.Services
{
    public class CapabilitiesService : ICapabilitiesService
    {
        private readonly DownloadContext _dbContext;
        private readonly IRegisterFetcher _registerFetcher;

        public CapabilitiesService(DownloadContext dbContextContext, IRegisterFetcher registerFetcherFetcher)
        {
            _dbContext = dbContextContext;
            _registerFetcher = registerFetcherFetcher;
        }

        public CapabilitiesType GetCapabilities(string metadataUuid) 
        {
            var dataset = GetDataset(metadataUuid);

            if (dataset == null) return null;

            return new CapabilitiesType
            {
                supportsAreaSelection = dataset.supportsAreaSelection.GetValueOrDefault(),
                supportsFormatSelection = dataset.supportsFormatSelection.GetValueOrDefault(),
                supportsPolygonSelection = dataset.supportsPolygonSelection.GetValueOrDefault(),
                supportsProjectionSelection = dataset.supportsProjectionSelection.GetValueOrDefault(),
                mapSelectionLayer = dataset.mapSelectionLayer,
                _links = new CapabilityLinksCreator().CreateCapabilityLinks(metadataUuid).ToArray()
            };
        }

        public Dataset GetDataset(string metadataUuid)
        {
            return (from c in _dbContext.Capabilities
                where c.metadataUuid == metadataUuid
                select c).FirstOrDefault();
        }

        public List<ProjectionType> GetProjections(string metadataUuid)
        {

            var query = (from p in _dbContext.FileList
                              where p.Dataset1.metadataUuid == metadataUuid
                              select new { p.projeksjon, p.format }).Distinct().ToList();

            var projectionsQuery = (from p in query
                                    select p.projeksjon).Distinct();

            List<ProjectionType> projections = new List<ProjectionType>();

            foreach (var projection in projectionsQuery.ToList())
            {

                ProjectionType p1 = new ProjectionType();
                p1.code = projection.ToString();
                p1.codespace = _registerFetcher.GetProjection(projection.ToString()).codespace;
                p1.name = _registerFetcher.GetProjection(projection.ToString()).name;

                var projectionFormats = query.Where(p => p.projeksjon == projection).Select(a => new { a.format }).Distinct();

                List<FormatType> formatList = new List<FormatType>();
                foreach (var format in projectionFormats)
                    formatList.Add(new FormatType { name = format.format });

                p1.formats = formatList.ToArray();

                projections.Add(p1);
            }

            return projections;
        }


        public List<AreaType> GetAreas(string metadataUuid)
        {
            var areasQuery = (from p in _dbContext.FileList
                              where p.Dataset1.metadataUuid == metadataUuid
                              select new { p.inndeling, p.inndelingsverdi, p.projeksjon, p.format }).Distinct().ToList() ;

            List<AreaType> areas = new List<AreaType>();

            foreach (var area in areasQuery.Select(a => new { a.inndeling, a.inndelingsverdi }).Distinct() )
            { 
            AreaType a1 = new AreaType();
            a1.type = area.inndeling;
            a1.code = area.inndelingsverdi;
            a1.name = _registerFetcher.GetArea(a1.type, a1.code).name;
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
                    var projectionFormats = areasQuery.Where(p => p.inndeling == type && p.inndelingsverdi == code && p.projeksjon == data.projeksjon).Select(a => new { a.format }).Distinct();

                    List<FormatType> formatList = new List<FormatType>();
                    foreach (var format in projectionFormats)
                        formatList.Add(new FormatType { name = format.format });

                    projections.Add(new ProjectionType
                    {
                        code = data.projeksjon,
                        codespace = _registerFetcher.GetProjection(data.projeksjon).codespace,
                        name = _registerFetcher.GetProjection(data.projeksjon).name,
                        formats = formatList.ToArray()
                    });
                }

                areas[i].projections = projections.ToArray();

                List<FormatType> formats = new List<FormatType>();

                foreach (var data in areasQuery.Where(p => p.inndeling == type && p.inndelingsverdi == code).Select(a => new { a.format }).Distinct())
                {
                    formats.Add(new FormatType { name = data.format });
                    {
                        var formatProjections = areasQuery.Where(p => p.inndeling == type && p.inndelingsverdi == code && p.format == data.format).Select(a => new { a.projeksjon }).Distinct();

                        List<ProjectionType> projectionList = new List<ProjectionType>();
                        foreach (var projection in formatProjections)
                            projectionList.Add(new ProjectionType
                            {
                              code = projection.projeksjon,
                              codespace = _registerFetcher.GetProjection(projection.projeksjon).codespace,
                              name = _registerFetcher.GetProjection(projection.projeksjon).name,
                            });

                        formats.Add(new FormatType { name = data.format, projections = projectionList.ToArray() });
                    }

                    areas[i].formats = formats.ToArray();
                }
            }

            return areas;
        }


        public List<FormatType> GetFormats(string metadataUuid)
        {

            var query = (from p in _dbContext.FileList
                         where p.Dataset1.metadataUuid == metadataUuid
                         select new { p.projeksjon, p.format }).Distinct().ToList();

            var formatsQuery = (from p in query
                              select p.format).Distinct();

            List<FormatType> formats = new List<FormatType>();

            foreach (var format in formatsQuery)
            {
                FormatType f1 = new FormatType();
                f1.name = format.ToString();

                var formatProjections = query.Where(p => p.format == format).Select(a => new { a.projeksjon }).Distinct();

                List<ProjectionType> projectionList = new List<ProjectionType>();
                foreach (var projection in formatProjections)
                    projectionList.Add(new ProjectionType
                    {
                        code = projection.projeksjon,
                        codespace = _registerFetcher.GetProjection(projection.projeksjon).codespace,
                        name = _registerFetcher.GetProjection(projection.projeksjon).name,
                    });

                f1.projections = projectionList.ToArray();

                formats.Add(f1);
            }


            return formats;
        }
        



    }
}