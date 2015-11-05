using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Geonorge.Download.Services
{
    public class RegisterFetcher
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
                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/sosi-kodelister/kartverket/fylkesnummer";
                System.Net.WebClient c = new System.Net.WebClient();
                c.Encoding = System.Text.Encoding.UTF8;
                var data = c.DownloadString(url);
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["codevalue"].ToString();
                    var label = code["label"].ToString();

                    AreaType fylke = new AreaType { code = codevalue, name = label, type = "fylke" };

                    areas.Add(fylke);
                }

                //kommune
                url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/sosi-kodelister/kartverket/kommunenummer";
                data = c.DownloadString(url);
                response = Newtonsoft.Json.Linq.JObject.Parse(data);

                codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["codevalue"].ToString();
                    var label = code["label"].ToString();

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
                area = new AreaType { code = code, name = code, type = type };

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