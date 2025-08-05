
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options; 
using System;
using System.Threading.Tasks;
using CEA_API.Configuration;
using CEA_API.Interfaces;

namespace CEA_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            await SendEmailInternalAsync(toEmail, subject, message, isHtml: false);
        }

        public async Task SendEmailWithHtmlAsync(string toEmail, string subject, string htmlMessage)
        {
            await SendEmailInternalAsync(toEmail, subject, htmlMessage, isHtml: true);
        }

        private async Task SendEmailInternalAsync(string toEmail, string subject, string content, bool isHtml)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailSettings.SenderEmail);
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (isHtml)
            {
                bodyBuilder.HtmlBody = content;
            }
            else
            {
                bodyBuilder.TextBody = content;
            }
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                // Conecta al servidor SMTP usando STARTTLS (recomendado para el puerto 587)
                await smtp.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);

                // Autentica con el nombre de usuario y la contraseña de aplicación
                await smtp.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

                // Envía el correo
                await smtp.SendAsync(email);
                Console.WriteLine($"Email sent to {toEmail} successfully."); // Mensaje para depuración
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email to {toEmail}: {ex.Message}");
                // Propaga la excepción para que el controlador pueda manejarla
                throw new ApplicationException($"Error al enviar el correo a {toEmail}: {ex.Message}", ex);
            }
            finally
            {
                // Desconecta el cliente SMTP
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
