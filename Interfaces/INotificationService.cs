using NyayaSetu.Models;

namespace NyayaSetu.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(string userId, string title, string message, NotificationType type, string? linkUrl = null);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int take = 30);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
