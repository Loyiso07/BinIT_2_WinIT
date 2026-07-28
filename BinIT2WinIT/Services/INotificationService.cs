using System.Threading.Tasks;
using BinIT2WinIT.Models;

namespace BinIT2WinIT.Services
{
    // ✅ ONLY ONE INTERFACE DEFINITION
    public interface INotificationService
    {
        Task SendNotification(string userId, string title, string message, string type = "General", string link = null);
        Task NotifyOfficerAssignment(int officerId, string regionName);
        Task NotifyPointsUpdate(int residentId, int points, string reason);
        Task SendEmailNotification(string email, string subject, string message);
        Task<int> GetUnreadCount(string userId);
    }
}