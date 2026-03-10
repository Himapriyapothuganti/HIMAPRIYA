using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> AddAsync(Notification notification);
        Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
        Task<Notification?> GetByIdAsync(int id);
        Task UpdateAsync(Notification notification);
    }
}
