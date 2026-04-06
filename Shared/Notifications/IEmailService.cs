using System.Threading.Tasks;

namespace AppointmentSystem.Api.Shared.Notifications
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string bodyHtml);
    }
}
