using Kartverket.Geonorge.Download.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Geonorge.NedlastingApi.V3;
using log4net;
using System.Net.Http;
using Kartverket.Geonorge.Download.Services.Auth;
using System;

namespace Kartverket.Geonorge.Download.Services
{
    public class CapabilitiesService : ICapabilitiesService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DownloadContext _dbContext;
        private readonly IRegisterFetcher _registerFetcher;
        private readonly IEiendomService _eiendomService;
        private readonly IAuthenticationService _authenticationService;

        public CapabilitiesService(DownloadContext dbContextContext, IRegisterFetcher registerFetcherFetcher, IAuthenticationService authenticationService, IEiendomService eiendomService)
        {
            _dbContext = dbContextContext;
            _registerFetcher = registerFetcherFetcher;
            _authenticationService = authenticationService;
            _eiendomService = eiendomService;
        }

        public CapabilitiesType GetCapabilities(string metadataUuid) 
        {
            var dataset = GetDataset(metadataUuid);

            if (dataset == null) return null;

            return new CapabilitiesType
            {
                supportsAreaSelection = dataset.SupportsAreaSelection.GetValueOrDefault(),
                supportsFormatSelection = dataset.SupportsFormatSelection.GetValueOrDefault(),
                supportsPolygonSelection = dataset.SupportsPolygonSelection.GetValueOrDefault(),
                supportsProjectionSelection = dataset.SupportsProjectionSelection.GetValueOrDefault(),
                supportsDownloadBundling = IsBundlingEnabled(),
                mapSelectionLayer = dataset.MapSelectionLayer,
                accessConstraintRequiredRole = dataset.AccessConstraintRequiredRole,
                distributedBy = ConfigurationManager.AppSettings["DistributedBy"],
                _links = new LinkCreator().CreateCapabilityLinks(metadataUuid).ToArray()
            };
        }

        private bool IsBundlingEnabled()
        {
            string configValue = ConfigurationManager.AppSettings["BundlingEnabled"];
            if (!bool.TryParse(configValue, out bool result))
                Log.Warn("Invalid configuration variable [BundlingEnabled]. Unable to parse boolean from value ["+ configValue +"]");
            return result;
        }

        public Dataset GetDataset(string metadataUuid)
        {
            return (from c in _dbContext.Capabilities
                where c.MetadataUuid == metadataUuid
                select c).FirstOrDefault();
        }

        public List<ProjectionType> GetProjections(string metadataUuid)
        {

            var query = (from p in _dbContext.FileList
                              where p.Dataset.MetadataUuid == metadataUuid
                              select new { projeksjon = p.Projection, format = p.Format }).Distinct().ToList();

            var projectionsQuery = (from p in query
                                    select p.projeksjon).Distinct();

            List<ProjectionType> projections = new List<ProjectionType>();

            foreach (var projection in projectionsQuery.ToList())
            {

                ProjectionType p1 = new ProjectionType();
                p1.code = projection;
                p1.codespace = _registerFetcher.GetProjection(projection).codespace;
                p1.name = _registerFetcher.GetProjection(projection).name;

                var projectionFormats = query.Where(p => p.projeksjon == projection).Select(a => new { a.format }).Distinct();

                List<FormatType> formatList = new List<FormatType>();
                foreach (var format in projectionFormats)
                    formatList.Add(new FormatType { name = format.format });

                p1.formats = formatList.ToArray();

                projections.Add(p1);
            }

            return projections;
        }


        public List<AreaType> GetAreas(string metadataUuid, HttpRequestMessage request = null)
        {
            string limitMunicipalityCode = "";

            var areasQuery = (from p in _dbContext.FileList
                              where p.Dataset.MetadataUuid == metadataUuid
                              select new { inndeling = p.Division, inndelingsverdi = p.DivisionKey, projeksjon = p.Projection, format = p.Format }).Distinct().ToList();

            var dataset = (from d in _dbContext.FileList
                                where d.Dataset.MetadataUuid == metadataUuid
                                select d.Dataset).FirstOrDefault();

            if(dataset != null && !string.IsNullOrEmpty(dataset.AccessConstraintRequiredRole)
                &&(dataset.AccessConstraintRequiredRole.Contains(AuthConfig.DatasetOnlyOwnMunicipalityRole)
                || dataset.AccessConstraintRequiredRole.Contains(AuthConfig.DatasetAgriculturalPartyRole))
                )
            {
                var user = _authenticationService.GetAuthenticatedUser(request);

              if (dataset.AccessConstraintRequiredRole.Contains(AuthConfig.DatasetAgriculturalPartyRole) &&
                    user.HasRole(AuthConfig.DatasetAgriculturalPartyRole))
                {

                    List<Eiendom> eiendoms = _eiendomService.GetEiendoms(user);

                    if (eiendoms == null)
                        eiendoms = new List<Eiendom>();

                    return GetAreasEiendoms(eiendoms, metadataUuid);

                    
                   
                }
                else if (dataset.AccessConstraintRequiredRole.Contains(AuthConfig.DatasetOnlyOwnMunicipalityRole) &&
                    user.HasRole(AuthConfig.DatasetOnlyOwnMunicipalityRole))
                {
                    limitMunicipalityCode = user.MunicipalityCode;
                    if (!string.IsNullOrEmpty(limitMunicipalityCode))
                    {
                            areasQuery = (from p in _dbContext.FileList
                                          where p.Dataset.MetadataUuid == metadataUuid
                                          && p.Division == "kommune" && p.DivisionKey == limitMunicipalityCode
                                          select new { inndeling = p.Division, inndelingsverdi = p.DivisionKey, projeksjon = p.Projection, format = p.Format }).Distinct().ToList();
                    }
                        
                }
            }

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
                         where p.Dataset.MetadataUuid == metadataUuid
                         select new { projeksjon = p.Projection, format = p.Format }).Distinct().ToList();

            var formatsQuery = (from p in query
                              select p.format).Distinct();

            List<FormatType> formats = new List<FormatType>();

            foreach (var format in formatsQuery)
            {
                FormatType f1 = new FormatType();
                f1.name = format;

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

        private List<AreaType> GetAreasEiendoms(List<Eiendom> eiendoms, string metadataUuid)
        {
            var municipalities = eiendoms.Select(e => e.kommnr).Distinct().ToArray();
            var areasQuery = (from p in _dbContext.FileList
                              where p.Dataset.MetadataUuid == metadataUuid
                              && p.Division == "kommune" && municipalities.Contains(p.DivisionKey)
                              select new { inndeling = p.Division, inndelingsverdi = p.DivisionKey, projeksjon = p.Projection, format = p.Format }).Distinct().ToList();

            List<AreaType> areas = new List<AreaType>();

            foreach (var area in areasQuery.Select(a => new { a.inndeling, a.inndelingsverdi }).Distinct())
            {
                AreaType a1 = new AreaType();
                a1.type = area.inndeling;
                a1.code = area.inndelingsverdi;
                a1.name = _registerFetcher.GetArea(area.inndeling, area.inndelingsverdi).name;
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

            List<AreaType> areasForEiendoms = new List<AreaType>();

            if(areas.Count > 3) { 
            AreaType national = new AreaType();
            national.type = "landsdekkende";
            national.code = "0000";
            national.name = "Alle mine kommuner";
            national.projections = areas.FirstOrDefault().projections;
            national.formats = areas.FirstOrDefault().formats;
            areasForEiendoms.Add(national);
            }

            foreach (var area in areas)
            {

                AreaType a1 = new AreaType();
                a1.type = area.type;
                a1.code = area.code;
                a1.name = _registerFetcher.GetArea(area.type, area.code).name;
                a1.projections = area.projections;
                a1.formats = area.formats;
                areasForEiendoms.Add(a1);
            }

            return areasForEiendoms;
        }


    }
}