using System.Net.Http;
using System.Threading.Tasks;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IExternalRequestService
    {
        Task<HttpResponseMessage> RunRequestAsync(string url);
    }
}