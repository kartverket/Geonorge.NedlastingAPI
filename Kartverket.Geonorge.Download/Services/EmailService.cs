using System.Net.Mail;
using System.Web.Configuration;

namespace Kartverket.Geonorge.Download.Services
{
    public class EmailService : IEmailService
    {
        public void Send(MailMessage message)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Host = WebConfigurationManager.AppSettings["SmtpHost"];

                try
                {
                    smtpClient.Send(message); // TODO: Send async (?)
                }
                catch (SmtpException)
                {
                }
            }
        }
    }
}
