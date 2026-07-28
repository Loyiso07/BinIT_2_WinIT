using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BinIT2WinIT.Data;
using BinIT2WinIT.Models;
using Microsoft.AspNet.Identity;

namespace BinIT2WinIT.Services
{
    // ✅ IMPLEMENTS INotificationService AND IDisposable
    public class NotificationService : INotificationService, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private bool _disposed = false;

        public NotificationService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SendNotification(string userId, string title, string message, string type = "General", string link = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Link = link,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await SendEmailNotification(user.Email, title, message);
            }
        }

        public async Task NotifyOfficerAssignment(int officerId, string regionName)
        {
            var officer = await _context.CollectionOfficers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OfficerId == officerId);

            if (officer == null || officer.User == null) return;

            var userId = officer.UserId;
            var title = "📍 New Region Assignment";
            var message = $"You have been assigned to {regionName}. Please check your dashboard for details.";
            var link = "/Officer/Dashboard";

            await SendNotification(userId, title, message, "RegionAssignment", link);
        }

        public async Task NotifyPointsUpdate(int residentId, int points, string reason)
        {
            var resident = await _context.Residents
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ResidentId == residentId);

            if (resident == null || resident.User == null) return;

            var userId = resident.UserId;
            var title = "⭐ Points Updated!";
            var message = $"{reason} You earned {points} points! Total points: {resident.PointsBalance}";
            var link = "/Resident/PointsHistory";

            await SendNotification(userId, title, message, "PointsUpdate", link);
        }

        public async Task SendEmailNotification(string email, string subject, string message)
        {
            try
            {
                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password");

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("noreply@binit2winit.co.za", "BinIT2WinIT"),
                        Subject = subject,
                        Body = $@"
                            <h2>BinIT2WinIT Notification</h2>
                            <p>{message}</p>
                            <hr />
                            <small>Visit your dashboard for more details.</small>
                        ",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);
                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email failed: {ex.Message}");
            }
        }

        public async Task<int> GetUnreadCount(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        // ✅ IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}