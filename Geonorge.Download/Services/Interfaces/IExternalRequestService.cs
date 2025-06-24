using System.Net.Http;
using System.Threading.Tasks;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IExternalRequestService
    {
        Task<HttpResponseMessage> RunRequestAsync(string url);
    }
}