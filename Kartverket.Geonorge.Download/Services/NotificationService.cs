using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using Kartverket.Geonorge.Download.Models;
using log4net;
using System.Linq;

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
            body.AppendLine().AppendLine("Linken for å laste ned, samt epost slettes etter 7 dager.");
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

        public void SendOrderInfoNotification(Order order, List<OrderItem> clippableOrderItems)
        {
            var message = CreateOrderInfoEmailMessage(order, clippableOrderItems);

            SendEmailNotification(message);
        }

        private MailMessage CreateOrderInfoEmailMessage(Order order, List<OrderItem> clippableOrderItems)
        {
            var email = order.email;
            var message = CreateEmail(email);
            var body = new StringBuilder();

            body.AppendLine(
                $"Din bestilling fra Geonorges kartkatalog med bestillingsnummer {order.referenceNumber} er under behandling.\n");

            body.AppendLine(
                $"NB! Noen klippejobber kan ta lang tid. Hvis de tar flere dager vil du få en daglig epost med status på jobben og lenker til filer som er ferdigprodusert.\n");

            body.AppendLine(
                $"Datasett som skal klippes:");

            foreach (var item in clippableOrderItems)
            {
                body.AppendLine($"{item.MetadataName} {item.AreaName}");
            }

            AddFooter(body);

            message.Body = body.ToString();

            Log.Info($"Sending info clippable objects email notification to: {email}, referenceNumber: {order.referenceNumber}");

            return message;
        }

        public void SendOrderStatusNotification(Order order)
        {
            var message = CreateOrderStatusEmailMessage(order);

            SendEmailNotification(message);
        }

        private MailMessage CreateOrderStatusEmailMessage(Order order)
        {
            var email = order.email;
            var message = CreateEmail(email);
            message.Subject = message.Subject + " - statusoppdatering";
            var body = new StringBuilder();

            body.AppendLine(
                $"Din bestilling fra Geonorges kartkatalog med bestillingsnummer {order.referenceNumber} er fortsatt under behandling.\n");

            body.AppendLine(
                $"Følgende datasett venter fortsatt på å bli behandlet:");

            foreach (var item in order.orderItem.Where(i => i.Status == OrderItemStatus.WaitingForProcessing))
            {
                body.AppendLine($"{item.MetadataName} {item.AreaName}");
            }

            body.AppendLine();

            var readyForDownload = order.orderItem.Where(i => i.Status == OrderItemStatus.ReadyForDownload && !(i.DownloadUrl == null || i.DownloadUrl.Trim() == string.Empty)).ToList();

            if (readyForDownload.Any()) { 
                body.AppendLine(
                $"Følgende datasett er klare til nedlasting:");

                foreach (var item in readyForDownload)
                {
                    body.AppendLine($"Datasett: {item.MetadataName}");

                    body.AppendLine("Klikk her for å laste ned resultatet: ");
                    var downLoadApiUrl = new DownloadUrlBuilder().OrderId(item.Order.Uuid).FileId(item.Uuid)
                        .Build();
                    body.AppendLine(downLoadApiUrl);
                }

            }

            if (readyForDownload.Any())
                AddPrivacyInfo(body);

            AddFooter(body);

            message.Body = body.ToString();

            Log.Info($"Sending OrderStatusEmailMessage to: {email}, referenceNumber: {order.referenceNumber}");

            return message;
        }

        public void SendOrderStatusNotificationNotDeliverable(Order order)
        {
            var message = CreateOrderStatusNotDeliverableEmailMessage(order);

            SendEmailNotification(message);
        }

        private MailMessage CreateOrderStatusNotDeliverableEmailMessage(Order order)
        {
            var email = order.email;
            var message = CreateEmail(email);
            message.Bcc.Add(new MailAddress(WebConfigurationManager.AppSettings["SupportEmail"]));
            message.Subject = message.Subject + " - produksjonen av datasett feilet";
            var body = new StringBuilder();

            body.AppendLine(
                $"Din bestilling fra Geonorges kartkatalog med bestillingsnummer {order.referenceNumber} feilet for følgende datasett:");

            foreach (var item in order.orderItem.Where(i => i.Status == OrderItemStatus.WaitingForProcessing))
            {
                body.AppendLine($"{item.MetadataName} {item.AreaName}");
                body.AppendLine($"Koordinater: {item?.Coordinates}");
                body.AppendLine($"Projeksjon: {item?.Projection} {item?.ProjectionName}");
                body.AppendLine($"Format: {item?.Format}");
                body.AppendLine($"MetadataUuid: {item?.MetadataUuid}");
                body.AppendLine($"Ordredato: {item?.Order?.orderDate}");
            }

            AddFooter(body);

            message.Body = body.ToString();

            Log.Info($"Sending CreateOrderStatusNotDeliverableEmailMessage to: {email}, referenceNumber: {order.referenceNumber}");

            return message;
        }
    }
}