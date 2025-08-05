using System.Threading.Tasks;

namespace CEA_API.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendEmailWithHtmlAsync(string toEmail, string subject, string htmlMessage);
    }
}
