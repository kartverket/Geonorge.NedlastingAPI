namespace Kartverket.Geonorge.Download.Services
{
    public interface INotificationService
    {
        void SendReadyForDownloadNotification(string fileId);
    }
}
