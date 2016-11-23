using System.Net.Mail;
using System.Reflection;
using System.Web.Configuration;
using log4net;

namespace Kartverket.Geonorge.Download.Services
{
    public class EmailService : IEmailService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Send(MailMessage message)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Host = WebConfigurationManager.AppSettings["SmtpHost"];
                smtpClient.Send(message); // TODO: Send async (?)
            }
        }
    }
}
