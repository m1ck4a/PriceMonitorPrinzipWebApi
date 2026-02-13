using PriceMonitorPrinzipWebApi.Services.Interfaces;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace PriceMonitorPrinzipWebApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPriceAddEmailAsync(string email, string adUrl, decimal Price)
        {
            try
            {
                var _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.mail.ru";
                var _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var _smtpUser = _configuration["EmailSettings:SmtpUser"] ?? throw new Exception("SMTP user not configured");
                var _smtpPass = _configuration["EmailSettings:SmtpPass"] ?? throw new Exception("SMTP password not configured");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Price Monitoring", _smtpUser));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "Подписка на квартиру";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <h2>Вы подписались на уведомление о изменение цены на квартиру!</h2>
                    <p>Ссылка на объявление: <a href='{adUrl}'>{adUrl}</a></p>
                    <p>Цена {Price} ₽</p>
                "
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_smtpUser, _smtpPass);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                Console.WriteLine($"Email отправлен по {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        public async Task SendPriceChangeEmailAsync(string email, string adUrl, decimal oldPrice, decimal newPrice)
        {
            try
            {
                var _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.mail.ru";
                var _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var _smtpUser = _configuration["EmailSettings:SmtpUser"] ?? throw new Exception("SMTP user not configured");
                var _smtpPass = _configuration["EmailSettings:SmtpPass"] ?? throw new Exception("SMTP password not configured");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Price Monitor", _smtpUser));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "Изменение цены на квартиру";

                string priceChange = newPrice > oldPrice ? "увеличилась" : "уменьшилась";
                string diff = Math.Abs(newPrice - oldPrice).ToString("N0");

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <h2>Цена изменилась!</h2>
                    <p>Ссылка на объявление: <a href='{adUrl}'>{adUrl}</a></p>
                    <p>Старая цена: <strong>{oldPrice:N0} ₽</strong></p>
                    <p>Новая цена: <strong>{newPrice:N0} ₽</strong></p>
                    <p>Цена {priceChange} на {diff} ₽</p>
                "
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_smtpUser, _smtpPass);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                Console.WriteLine($"Email отправлен на {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки сообщения: {ex.Message}");
                // В продакшене нужно логировать ошибку
            }
        }
    }
}
