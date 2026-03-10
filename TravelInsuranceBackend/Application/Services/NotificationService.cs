using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<NotificationDTO> CreateNotificationAsync(string userId, string message, string type = "System")
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);

            return MapToDTO(notification);
        }

        public async Task<IEnumerable<NotificationDTO>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            return notifications.Select(MapToDTO);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null || notification.UserId != userId)
            {
                return false;
            }

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
            return true;
        }

        private static NotificationDTO MapToDTO(Notification entity)
        {
            return new NotificationDTO
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Message = entity.Message,
                IsRead = entity.IsRead,
                CreatedAt = entity.CreatedAt,
                Type = entity.Type
            };
        }
    }
}
