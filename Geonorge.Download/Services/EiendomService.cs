using Geonorge.Download.Models;
using Geonorge.Download.Services.Interfaces;

namespace Geonorge.Download.Services
{
    public class EiendomService(ILogger<EiendomService> logger, IConfiguration config) : IEiendomService
    {

        public List<Eiendom> GetEiendoms(AuthenticatedUser user)
        {
            List<Eiendom> eiendoms = [];
            try
            {
                using (var client = new HttpClient())
                {
                    var url = config["MatrikkelEiendomEndpoint"] + "/" + user.Username.ToUpper();
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("x-api-key", config["MatrikkelEiendomEndpointToken"]);

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
                                logger.LogError($"Error nibio api: {e}, content: {result.Content.ReadAsStringAsync().Result}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error nibio api: {ex}");
            }

            return eiendoms;
        }
    }
}