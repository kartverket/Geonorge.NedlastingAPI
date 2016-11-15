using System.Web;
using Geonorge.NedlastingApi.V2;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IDownloadService
    {
        FileType GetFileType(OrderReceiptType order, int fileId);
        bool IsReadyToDownload(FileType file);
        HttpResponse CreateResponseFromRemoteFile(string url);
    }
}
