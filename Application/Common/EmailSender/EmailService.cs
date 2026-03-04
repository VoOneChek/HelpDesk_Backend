using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Application.Common.EmailSender
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            _logger.LogInformation("Начинается отправка письма на адрес {To} с темой \"{Subject}\"", to, subject);

            try
            {
                var smtpHost = _config["SmtpClientHost"];
                var smtpPort = int.Parse(_config["SmtpClientPort"]!);
                var smtpUser = _config["MailLogin"];
                var smtpPass = _config["MailPassword"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPass)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(smtpUser!, "GradeBook"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                message.To.Add(to);
                await client.SendMailAsync(message);

                _logger.LogInformation("Письмо успешно отправлено на адрес {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке письма на адрес {To}", to);
                throw;
            }
        }
    }
}
