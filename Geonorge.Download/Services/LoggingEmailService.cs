using Geonorge.Download.Services.Interfaces;
using System.Net.Mail;

namespace Geonorge.Download.Services
{
    /// <summary>
    /// Used when GraphMail is not configured (local development). Logs instead of sending.
    /// Recipients and body are not logged since they contain personal data.
    /// </summary>
    public sealed class LoggingEmailService(ILogger<LoggingEmailService> logger) : IEmailService
    {
        public Task Send(MailMessage message)
        {
            logger.LogInformation("GraphMail not configured, skipping email with subject: {Subject}, recipients: {RecipientCount}",
                message.Subject, message.To.Count + message.Bcc.Count);

            return Task.CompletedTask;
        }
    }
}
