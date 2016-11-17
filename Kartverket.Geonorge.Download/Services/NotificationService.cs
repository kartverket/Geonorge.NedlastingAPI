using System.Net.Mail;
using System.Text;
using System.Web.Configuration;
using Kartverket.Geonorge.Download.Models;

namespace Kartverket.Geonorge.Download.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDownloadService _downloadService;
        private readonly IEmailService _emailService;
        private readonly IOrderService _orderService;

        public NotificationService
        (
            IDownloadService downloadService,
            IOrderService orderService,
            IEmailService emailService
        )
        {
            _downloadService = downloadService;
            _orderService = orderService;
            _emailService = emailService;
        }

        public void SendReadyForDownloadNotification(string fileId)
        {
            var message = CreateReadyForDownloadEmailMessage(fileId);

            SendEmailNotification(message);
        }

        public MailMessage CreateReadyForDownloadEmailMessage(string fileId)
        {
            var orderItem = _orderService.FindOrderItem(fileId);

            var message = new MailMessage();
            message.To.Add(new MailAddress(orderItem.Order.email));
            message.From = new MailAddress(WebConfigurationManager.AppSettings["WebmasterEmail"]);
            message.Subject = "Fil klar for nedlasting";
            var body = new StringBuilder();
            body.Append(orderItem.FileName + " er klar for nedlasting.");

            var downLoadApiUrl = new DownloadUrlBuilder().OrderId(orderItem.Order.Uuid).FileId(orderItem.FileId).Build();
            
            body.Append(" <a href=\"" + downLoadApiUrl + "\">Klikk her for å laste ned</a>");

            message.Body = body.ToString();
            message.IsBodyHtml = true;

            return message;
        }

        public void SendEmailNotification(MailMessage message)
        {
            _emailService.Send(message);
        }
    }
}
