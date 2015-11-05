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

        public RegisterFetcher()
        {
            Areas = GetAreas();
            Projections = GetProjections();
        }

        public List<AreaType> GetAreas()
        {
            List<AreaType> areas = new List<AreaType>();

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
            List<ProjectionType> projections = new List<ProjectionType>();


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