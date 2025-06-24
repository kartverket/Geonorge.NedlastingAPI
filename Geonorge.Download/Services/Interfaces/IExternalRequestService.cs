namespace Geonorge.Download.Services.Interfaces
{
    public interface IExternalRequestService
    {
        Task<HttpResponseMessage> RunRequestAsync(string url);
    }
}