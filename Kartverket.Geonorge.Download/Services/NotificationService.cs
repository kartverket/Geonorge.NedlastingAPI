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

        private readonly IEmailService _emailService;

        public NotificationService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public void SendReadyForDownloadNotification(OrderItem orderItem)
        {
            var message = CreateReadyForDownloadEmailMessage(orderItem);

            SendEmailNotification(message);
        }

        public void SendReadyForDownloadBundleNotification(Order order)
        {
            var message = CreateReadyForDownloadBundleEmailMessage(order);

            SendEmailNotification(message);
        }

        public MailMessage CreateReadyForDownloadBundleEmailMessage(Order order)
        {
            var message = CreateEmail(order.email);
            var body = new StringBuilder();
            body.AppendLine(
                $"Din bestilling fra Geonorges kartkatalog med bestillingsnummer {order.referenceNumber} er ferdig produsert.\n");

            body.AppendLine("Du har bestilt en nedlastingspakke som inneholder følgende data:\n");

            foreach (var orderItem in order.orderItem)
            {
                body.Append($"* {orderItem.MetadataName} (");

                if (!string.IsNullOrEmpty(orderItem.AreaName))
                    body.Append(orderItem.AreaName);
                else
                    body.Append("utsnitt valgt i kart");

                body.AppendLine($" , {orderItem.ProjectionName}, {orderItem.Format})");
            }

            body.AppendLine("\nKlikk her for å laste ned resultatet: ");
            var downLoadApiUrl = new DownloadUrlBuilder().OrderId(order.Uuid).AsBundle();
            body.AppendLine(downLoadApiUrl);

            AddPrivacyInfo(body);

            AddFooter(body);

            message.Body = body.ToString();

            Log.Info($"Sending ReadyForDownload email notification to: {order.email}, orderUuid: {order.Uuid}");

            return message;
        }

        private void AddPrivacyInfo(StringBuilder body)
        {
            body.AppendLine().AppendLine("Linken for å laste ned, samt epost slettes etter 1 dag.");
        }

        private void AddFooter(StringBuilder body)
        {
            body.AppendLine().AppendLine("--").AppendLine("Med vennlig hilsen").AppendLine("Geonorge");
        }

        private static MailMessage CreateEmail(string email)
        {
            var message = new MailMessage();
            message.To.Add(new MailAddress(email));
            message.From = new MailAddress(WebConfigurationManager.AppSettings["WebmasterEmail"]);
            message.Subject = "Data til nedlasting fra Geonorge";
            return message;
        }

        public MailMessage CreateReadyForDownloadEmailMessage(OrderItem orderItem)
        {
            var email = orderItem.Order.email;
            var message = CreateEmail(email);
            var body = new StringBuilder();
            body.AppendLine(
                $"Din bestilling fra Geonorges kartkatalog med bestillingsnummer {orderItem.ReferenceNumber} er ferdig produsert.\n");
            foreach(var item in orderItem.Order.orderItem)
            { 
                body.AppendLine($"Datasett: {item.MetadataName}\n");

                if (!string.IsNullOrEmpty(item.DownloadUrl))
                {
                    body.AppendLine("Klikk her for å laste ned resultatet: ");
                    var downLoadApiUrl = new DownloadUrlBuilder().OrderId(item.Order.Uuid).FileId(item.Uuid)
                        .Build();
                    body.AppendLine(downLoadApiUrl);
                    AddPrivacyInfo(body);
                }
                else if (item.Status == OrderItemStatus.Error)
                {
                    body.AppendLine(
                        "Produksjonen av datasettet feilet.");
                }
                else
                {
                    body.AppendLine(
                        "Det er dessverre ingen data å laste ned. Det finnes ingen objekter innenfor valgt område.");
                }

                body.AppendLine("\n");
            }
            AddFooter(body);

            message.Body = body.ToString();

            Log.Info($"Sending ReadyForDownload email notification to: {email}, fileId: {orderItem.Uuid}");

            return message;
        }

        public void SendEmailNotification(MailMessage message)
        {
            _emailService.Send(message);
        }
    }
}