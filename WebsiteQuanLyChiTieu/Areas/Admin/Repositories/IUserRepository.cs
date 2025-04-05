using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteQuanLyChiTieu.Areas.Admin.Models;

namespace WebsiteQuanLyChiTieu.Areas.Admin.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser> GetByIdAsync(string id);
        Task<bool> CreateAsync(ApplicationUser user, string password);
        Task<bool> UpdateAsync(ApplicationUser user);
        Task<bool> DeleteAsync(string id);
        Task<bool> AssignRoleAsync(ApplicationUser user, string role);
    }
}
