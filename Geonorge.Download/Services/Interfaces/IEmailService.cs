using System.Net.Mail;

namespace Geonorge.Download.Services.Interfaces
{
    public interface IEmailService
    {
        void Send(MailMessage message);
    }
}
