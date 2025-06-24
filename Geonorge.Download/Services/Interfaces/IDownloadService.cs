using System;
using System.Web;
using Geonorge.NedlastingApi.V2;
using Geonorge.Download.Models;
using Newtonsoft.Json.Linq;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IDownloadService
    {
        Task StreamRemoteFileToResponseAsync(HttpContext context, string url);
        JObject CallClipperFileChecker(string url);
        bool AreaIsWithinDownloadLimits(string coordinates, string coordinateSystem, string metadataUuid);
    }
}
