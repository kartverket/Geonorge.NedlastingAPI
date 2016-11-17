using System.Net.Mail;

namespace Kartverket.Geonorge.Download.Services
{
    public interface IEmailService
    {
        void Send(MailMessage message);
    }
}
