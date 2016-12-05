using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using Kartverket.Geonorge.Download.Models;
using log4net;

namespace Kartverket.Geonorge.Download.Services
{
    public class NotificationService : INotificationService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            var email = orderItem.Order.email;
            message.To.Add(new MailAddress(email));
            message.From = new MailAddress(WebConfigurationManager.AppSettings["WebmasterEmail"]);
            message.Subject = "Data til nedlasting fra Geonorge";
            var body = new StringBuilder();
            body.AppendLine($"Din bestilling fra Geonorges kartkatalog med bestillingsnummer {orderItem.ReferenceNumber} er ferdig produsert.\n");
            body.AppendLine($"Datasett: {orderItem.MetadataName}\n");

            if (!string.IsNullOrEmpty(orderItem.DownloadUrl))
            {
                body.AppendLine("Klikk her for å laste ned resultatet: ");
                var downLoadApiUrl = new DownloadUrlBuilder().OrderId(orderItem.Order.Uuid).FileId(orderItem.FileId).Build();
                body.AppendLine(downLoadApiUrl);
            }
            else
            {
                body.AppendLine("Det er dessverre ingen data å laste ned. Det finnes ingen objekter innenfor valgt område.");
            }

            message.Body = body.ToString();

            Log.Info($"Sending ReadyForDownload email notification to: {email}, fileId: {fileId}");

            return message;
        }

        public void SendEmailNotification(MailMessage message)
        {
            _emailService.Send(message);
        }
    }
}
