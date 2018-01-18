using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.NedlastingApi.V1;

namespace Kartverket.Geonorge.Download.Services
{
    public class RegisterFetcher : IRegisterFetcher
    {

        List<AreaType> Areas = new List<AreaType>();
        List<ProjectionType> Projections = new List<ProjectionType>();

        MemoryCacher memCacher = new MemoryCacher();

        public RegisterFetcher()
        {
            Areas = GetAreas();
            Projections = GetProjections();
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
                
                //fylke
                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/sosi-kodelister/kartverket/fylkesnummer-alle";
                System.Net.WebClient c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;
                var data = c.DownloadString(url);
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["label"].ToString();
                    var label = code["description"].ToString();

                    AreaType fylke = new AreaType { code = codevalue, name = label, type = "fylke" };

                    areas.Add(fylke);
                }

                //kommune
                url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/sosi-kodelister/kartverket/kommunenummer-alle";
                data = c.DownloadString(url);
                response = Newtonsoft.Json.Linq.JObject.Parse(data);

                codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["label"].ToString();
                    var label = code["description"].ToString();

                    AreaType kommune = new AreaType { code = codevalue, name = label, type = "kommune" };

                    areas.Add(kommune);
                }

                //område utenfor fastlandsnorge
                url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/sosi-kodelister/kartverket/omradenummer";
                data = c.DownloadString(url);
                response = Newtonsoft.Json.Linq.JObject.Parse(data);

                codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["label"].ToString();
                    var label = code["description"]?.ToString();
                    var status = code["status"]?.ToString();

                    AreaType omraade;
                    if(codevalue.Length == 2)
                        omraade = new AreaType { code = codevalue, name = label, type = "fylke" };
                    else
                        omraade = new AreaType { code = codevalue, name = label, type = "kommune" };

                    areas.Add(omraade);
                }

                //fylke old
                url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/sosi-kodelister/kartverket/fylkesnummer";
                c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;
                data = c.DownloadString(url);
                response = Newtonsoft.Json.Linq.JObject.Parse(data);

                codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["codevalue"].ToString();
                    var label = code["label"].ToString();

                    AreaType fylke = new AreaType { code = codevalue, name = label, type = "fylke" };
                    var areaExists = areas.Where(a => a.code == codevalue && a.type == "fylke").FirstOrDefault();
                    if(areaExists == null)
                        areas.Add(fylke);
                }

                //kommune old
                url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/sosi-kodelister/kartverket/kommunenummer";
                data = c.DownloadString(url);
                response = Newtonsoft.Json.Linq.JObject.Parse(data);

                codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["codevalue"].ToString();
                    var label = code["label"].ToString();

                    AreaType kommune = new AreaType { code = codevalue, name = label, type = "kommune" };
                    var areaExists = areas.Where(a => a.code == codevalue && a.type == "kommune").FirstOrDefault();
                    if (areaExists == null)
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

                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/epsg-koder";
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

    }
}