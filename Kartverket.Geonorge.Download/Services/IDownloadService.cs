using System.Web;
using Geonorge.NedlastingApi.V2;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IDownloadService
    {
        HttpResponse CreateResponseFromRemoteFile(string url);
        bool AreaIsWithinDownloadLimits(string coordinates, string coordinateSystem, string metadataUuid);
    }
}
