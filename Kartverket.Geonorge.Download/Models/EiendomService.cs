using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;

namespace Kartverket.Geonorge.Download.Models
{
    public class EiendomService : IEiendomService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly HttpClient _httpClient;

        public EiendomService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<Eiendom> GetEiendoms(AuthenticatedUser user)
        {
            List<Eiendom> eiendoms = null ;
            try
            {
                using (var client = new HttpClient())
                {
                    var url = ConfigurationManager.AppSettings["MatrikkelEiendomEndpoint"] + "/" + user.Username.ToUpper();
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("x-api-key", ConfigurationManager.AppSettings["MatrikkelEiendomEndpointToken"]);

                    using (var response = client.GetAsync(url))
                    {
                        using (var result = response.Result)
                        {
                            try
                            {
                                eiendoms = result.Content.ReadAsAsync<List<Eiendom>>().Result;
                            }
                            catch(Exception e) 
                            {
                                Log.Error($"Error nibio api: {e}, content: {result.Content.ReadAsStringAsync().Result}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error nibio api: {ex}");
            }

            return eiendoms;
        }
    }

    public interface IEiendomService
    {
        List<Eiendom> GetEiendoms(AuthenticatedUser user);
    }
}