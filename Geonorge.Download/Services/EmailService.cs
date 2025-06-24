using Geonorge.Download.Services.Interfaces;
using System.Net.Mail;
using System.Reflection;

namespace Geonorge.Download.Services
{
    public class EmailService(IConfiguration config) : IEmailService
    {
        public void Send(MailMessage message)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Host = config["SmtpHost"];
                smtpClient.Send(message); // TODO: Send async (?)
            }
        }
    }
}
