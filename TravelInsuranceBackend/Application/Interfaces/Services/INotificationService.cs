using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<NotificationDTO> CreateNotificationAsync(string userId, string message, string type = "System");
        Task<IEnumerable<NotificationDTO>> GetUserNotificationsAsync(string userId);
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
    }
}
