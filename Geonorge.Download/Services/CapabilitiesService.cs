using Geonorge.Download.Models;
using Geonorge.Download.Services.Auth;
using Geonorge.Download.Services.Interfaces;
using Geonorge.NedlastingApi.V3;

namespace Geonorge.Download.Services
{
    public class CapabilitiesService(ILogger<CapabilitiesService> logger, IConfiguration config, DownloadContext dbContext, IRegisterFetcher registerFetcher, IEiendomService eiendomService, IAuthenticationService authenticationService) : ICapabilitiesService
    {

        public CapabilitiesType GetCapabilities(HttpRequest request, string metadataUuid) 
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
                distributedBy = config["DistributedBy"],
                _links = new LinkCreator().CreateCapabilityLinks(config, request, metadataUuid).ToArray()
            };
        }

        private bool IsBundlingEnabled()
        {
            string configValue = config["BundlingEnabled"];
            if (!bool.TryParse(configValue, out bool result))
                logger.LogWarning("Invalid configuration variable [BundlingEnabled]. Unable to parse boolean from value ["+ configValue +"]");
            return result;
        }

        public Dataset GetDataset(string metadataUuid)
        {
            return dbContext.Capabilities.FirstOrDefault(x => x.MetadataUuid == metadataUuid);
        }

        public List<ProjectionType> GetProjections(string metadataUuid)
        {
            var files = dbContext.FileList
                .Where(p => p.Dataset.MetadataUuid == metadataUuid)
                .Select(p => new { p.Projection, p.Format })
                .Distinct()
                .ToList();

            var projections = files
                .GroupBy(f => f.Projection)
                .Select(g => {
                    var projInfo = registerFetcher.GetProjection(g.Key);
                    return new ProjectionType
                    {
                        code = g.Key,
                        codespace = projInfo.codespace,
                        name = projInfo.name,
                        formats = g
                            .Select(f => f.Format)
                            .Distinct()
                            .Select(format => new FormatType { name = format })
                            .ToArray()
                    };
                })
                .ToList();


            if (projections.Count == 0 && metadataUuid == "b49478fd-038e-4c2c-ae28-dda1958a8048")
            {
                //Todo get default projections
                projections = GetProjections("595e47d9-d201-479c-a77d-cbc1f573a76b");
            }

            return projections;
        }


        public List<AreaType> GetAreas(string metadataUuid, HttpRequest request = null)
        {
            string limitMunicipalityCode = "";

            var areasQuery = (from p in dbContext.FileList
                              where p.Dataset.MetadataUuid == metadataUuid
                              select new { inndeling = p.Division, inndelingsverdi = p.DivisionKey, projeksjon = p.Projection, format = p.Format }).Distinct().ToList();

            var dataset = (from d in dbContext.FileList
                                where d.Dataset.MetadataUuid == metadataUuid
                                select d.Dataset).FirstOrDefault();

            if(dataset != null && !string.IsNullOrEmpty(dataset.AccessConstraintRequiredRole)
                &&(dataset.AccessConstraintRequiredRole.Contains(AuthConfig.DatasetOnlyOwnMunicipalityRole)
                || dataset.AccessConstraintRequiredRole.Contains(AuthConfig.DatasetAgriculturalPartyRole))
                )
            {
                var user = authenticationService.GetAuthenticatedUser(request);

                if (user == null)
                    throw new UnauthorizedAccessException("Bruker har ikke tilgang");

              if (dataset.AccessConstraintRequiredRole.Contains(AuthConfig.DatasetAgriculturalPartyRole) &&
                    user.HasRole(AuthConfig.DatasetAgriculturalPartyRole))
                {

                    List<Eiendom> eiendoms = eiendomService.GetEiendoms(user);

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
                            areasQuery = (from p in dbContext.FileList
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
            a1.name = registerFetcher.GetArea(a1.type, a1.code).name;
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
                        codespace = registerFetcher.GetProjection(data.projeksjon).codespace,
                        name = registerFetcher.GetProjection(data.projeksjon).name,
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
                              codespace = registerFetcher.GetProjection(projection.projeksjon).codespace,
                              name = registerFetcher.GetProjection(projection.projeksjon).name,
                            });

                        formats.Add(new FormatType { name = data.format, projections = projectionList.ToArray() });
                    }

                    areas[i].formats = formats.ToArray();
                }
            }

            if(areas.Count == 0 && metadataUuid == "b49478fd-038e-4c2c-ae28-dda1958a8048") 
            {
                //Todo handle only clipping
                areas = new List<AreaType> { new AreaType { code = "", name= "" } };
            }

            return areas;
        }

        public List<FormatType> GetFormats(string metadataUuid)
        {

            var query = (from p in dbContext.FileList
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
                        codespace = registerFetcher.GetProjection(projection.projeksjon).codespace,
                        name = registerFetcher.GetProjection(projection.projeksjon).name,
                    });

                f1.projections = projectionList.ToArray();

                formats.Add(f1);
            }

            if(formats.Count == 0 && metadataUuid == "b49478fd-038e-4c2c-ae28-dda1958a8048") 
            {
                //Todo get default formats
                formats = GetFormats("595e47d9-d201-479c-a77d-cbc1f573a76b");
            }

            return formats;
        }

        private List<AreaType> GetAreasEiendoms(List<Eiendom> eiendoms, string metadataUuid)
        {
            var municipalities = eiendoms.Select(e => e.kommunenr).Distinct().ToArray();
            var areasQuery = (from p in dbContext.FileList
                              where p.Dataset.MetadataUuid == metadataUuid
                              && p.Division == "kommune" && municipalities.Contains(p.DivisionKey)
                              select new { inndeling = p.Division, inndelingsverdi = p.DivisionKey, projeksjon = p.Projection, format = p.Format }).Distinct().ToList();

            List<AreaType> areas = new List<AreaType>();

            foreach (var area in areasQuery.Select(a => new { a.inndeling, a.inndelingsverdi }).Distinct())
            {
                AreaType a1 = new AreaType();
                a1.type = area.inndeling;
                a1.code = area.inndelingsverdi;
                a1.name = registerFetcher.GetArea(area.inndeling, area.inndelingsverdi).name;
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
                        codespace = registerFetcher.GetProjection(data.projeksjon).codespace,
                        name = registerFetcher.GetProjection(data.projeksjon).name,
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
                                codespace = registerFetcher.GetProjection(projection.projeksjon).codespace,
                                name = registerFetcher.GetProjection(projection.projeksjon).name,
                            });

                        formats.Add(new FormatType { name = data.format, projections = projectionList.ToArray() });
                    }

                    areas[i].formats = formats.ToArray();
                }
            }

            List<AreaType> areaEiendoms = new List<AreaType>();

            foreach (var eiendom in eiendoms)
            {

                var area = areas.Where(a => a.code == eiendom.kommunenr).FirstOrDefault();

                if(area != null)
                { 
                    AreaType a1 = new AreaType();
                    a1.type = "Eiendommer";
                    a1.code = $"{eiendom.kommunenr}/{eiendom.gaardsnr}/{eiendom.bruksnr}/{eiendom.festenr}";
                    a1.name = $"{registerFetcher.GetArea("kommune", eiendom.kommunenr).name}-{eiendom.gaardsnr}/{eiendom.bruksnr}/{eiendom.festenr}";
                    a1.projections = area?.projections;
                    a1.formats = area?.formats;
                    areaEiendoms.Add(a1);
                }
                else
                {
                    logger.LogWarning("Municipality: " + eiendom.kommunenr +  " not found in dataset area for nibio eiendoms");
                }
            }

            foreach (var municipality in eiendoms.Select(k => k.kommunenr).Distinct())
            {

                var area = areas.Where(a => a.code == municipality).FirstOrDefault();

                if (area != null)
                {
                    AreaType a1 = new AreaType();
                    a1.type = "Kommuner";
                    a1.code = municipality;
                    a1.name = $"{registerFetcher.GetArea("kommune", municipality).name}";
                    a1.projections = area?.projections;
                    a1.formats = area?.formats;
                    areaEiendoms.Add(a1);
                }
                else
                {
                    logger.LogWarning("Municipality: " + municipality + " not found in dataset area for nibio eiendoms");
                }
            }

            AreaType aNational = new AreaType();
            aNational.type = "Nasjonalt";
            aNational.code = "0000";
            aNational.name = "Alle eiendommer";
            aNational.projections = areas.FirstOrDefault().projections;
            aNational.formats = areas.FirstOrDefault().formats;
            areaEiendoms.Add(aNational);

            return areaEiendoms.OrderBy(o => o.name).ToList();
        }

        public void SaveClipperFile(Guid id, string url, bool valid, string message)
        {
            ClipperFile clipperFile = new ClipperFile();
            clipperFile.Id = id;
            clipperFile.DateUploaded = DateTime.UtcNow;
            clipperFile.File = url;
            clipperFile.Valid = valid;
            clipperFile.Message = message;

            dbContext.ClipperFiles.Add(clipperFile);
            dbContext.SaveChanges();
        }
    }
}