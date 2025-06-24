using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.NedlastingApi.V1;
using Geonorge.Download.Models;
using Microsoft.Extensions.Caching.Memory;
using Geonorge.Download.Services.Interfaces;
using Geonorge.Download.Services.Misc;

namespace Geonorge.Download.Services
{
    public class RegisterFetcher : IRegisterFetcher
    {
        IConfiguration config;

        List<AreaType> Areas = new List<AreaType>();
        List<ProjectionType> Projections = new List<ProjectionType>();
        List<Organization> Organizations = new List<Organization>();

        MemoryCacher memCacher = new MemoryCacher(new MemoryCache(new MemoryCacheOptions()));

        public RegisterFetcher(IConfiguration config)
        {
            this.config = config;
            Areas = GetAreas();
            Projections = GetProjections();
            Organizations = GetOrganizations();
        }

        public List<AreaType> GetAreas()
        {
            var cache = memCacher.GetValue("areas");

            List<AreaType> areas = new List<AreaType>();

            if (cache != null)
            {
                areas = cache as List<AreaType>;
            }
            else { 
                

                //område utenfor fastlandsnorge
                string url = config["RegistryUrl"] + "api/subregister/sosi-kodelister/kartverket/omradenummer";
                System.Net.WebClient c = new System.Net.WebClient();
                var data = c.DownloadString(url);
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["label"].ToString();
                    var label = code["description"]?.ToString();
                    var status = code["status"]?.ToString();
                    if (status == "Tilbaketrukket")
                        label = label + " (gammel)";
                    else if (status == "Sendt inn" || status == "Utkast")
                        label = label + " (ny)";

                    AreaType omraade;
                    if(codevalue.Length == 2)
                        omraade = new AreaType { code = codevalue, name = label, type = "fylke" };
                    else
                        omraade = new AreaType { code = codevalue, name = label, type = "kommune" };

                    areas.Add(omraade);
                }

                //fylke
                url = config["RegistryUrl"] + "api/sosi-kodelister/inndelinger/inndelingsbase/fylkesnummer";
                c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;
                data = c.DownloadString(url);
                response = Newtonsoft.Json.Linq.JObject.Parse(data);

                codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["codevalue"].ToString();
                    var label = code["label"].ToString();
                    var status = code["status"]?.ToString();
                    if (status == "Tilbaketrukket")
                        label = label + " (gammel)";
                    else if (status == "Sendt inn" || status == "Utkast")
                        label = label + " (ny)";

                    AreaType fylke = new AreaType { code = codevalue, name = label, type = "fylke" };

                    areas.Add(fylke);
                }

                //kommune
                url = config["RegistryUrl"] + "api/sosi-kodelister/inndelinger/inndelingsbase/kommunenummer";
                data = c.DownloadString(url);
                response = Newtonsoft.Json.Linq.JObject.Parse(data);

                codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["codevalue"].ToString();
                    var label = code["label"].ToString();
                    var status = code["status"]?.ToString();
                    if (status == "Tilbaketrukket")
                        label = label + " (gammel)";
                    else if (status == "Sendt inn" || status == "Utkast")
                        label = label + " (ny)";

                    AreaType kommune = new AreaType { code = codevalue, name = label, type = "kommune" };

                    areas.Add(kommune);
                }



                memCacher.Add("areas", areas, new DateTimeOffset(DateTime.Now.AddHours(1)));
            }

            return areas;
        }

        public AreaType GetArea(string type, string code)
        {
            AreaType area = Areas.Where(a => a.type == type && a.code == code).FirstOrDefault();
            if (area == null)
            { 
                if(code == "0000")
                    area = new AreaType { code = code, name = "Hele landet", type = type };
                else if (code == "90")
                    area = new AreaType { code = code, name = "Utland", type = type };
                else
                    area = new AreaType { code = code, name = code, type = type };
            }

            return area;
        }

        public List<ProjectionType> GetProjections()
        {
            var cache = memCacher.GetValue("projections");

            List<ProjectionType> projections = new List<ProjectionType>();

            if (cache != null)
            {
                projections = cache as List<ProjectionType>;
            }
            else
            {

                string url = config["RegistryUrl"] + "api/register/epsg-koder";
                System.Net.WebClient c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;
                var data = c.DownloadString(url);
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["documentreference"].ToString();
                    string[] details = codevalue.Split('/');
                    string epsgcode = details[details.Length - 1];
                    var label = code["label"].ToString();

                    ProjectionType projection = new ProjectionType { code = epsgcode, name = label, codespace = codevalue };

                    projections.Add(projection);
                }

                memCacher.Add("projections", projections, new DateTimeOffset(DateTime.Now.AddHours(1)));
            }

            return projections;
        }

        public ProjectionType GetProjection(string code)
        {
            ProjectionType projection = Projections.Where(p => p.code == code).FirstOrDefault();
            if (projection == null)
                projection = new ProjectionType { code = code, name = code };

            return projection;
        }

        public List<Organization> GetOrganizations()
        {
            var cache = memCacher.GetValue("organizations");

            List<Organization> organizations = new List<Organization>();

            if (cache != null)
            {
                organizations = cache as List<Organization>;
            }
            else
            {

                string url = config["RegistryUrl"] + "api/organisasjoner";
                System.Net.WebClient c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;
                var data = c.DownloadString(url);
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var name = code["label"]?.ToString();
                    var number = code["number"]?.ToString();
                    var municipalityCode = code["MunicipalityCode"]?.ToString();

                    Organization organization = new Organization { Name = name, Number = number, MunicipalityCode = municipalityCode };

                    organizations.Add(organization);
                }

                memCacher.Add("organizations", organizations, new DateTimeOffset(DateTime.Now.AddHours(1)));
            }

            return organizations;
        }

        public Organization GetOrganization(string organizationNumber)
        {
            Organization organization = Organizations.Where(o => o.Number == organizationNumber).FirstOrDefault();
            return organization;
        }

    }
}