using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetByIdAsync(string id);
        Task<ApplicationUser> GetByEmailAsync(string email);
        Task<List<ApplicationUser>> GetAllAsync();
        Task<List<ApplicationUser>> GetByRoleAsync(string role);
        Task<ApplicationUser> UpdateAsync(ApplicationUser user);
    }
}
