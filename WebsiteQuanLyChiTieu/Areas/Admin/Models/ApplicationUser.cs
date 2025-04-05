using Microsoft.AspNetCore.Identity;
using WebsiteQuanLyChiTieu.Models;

namespace WebsiteQuanLyChiTieu.Areas.Admin.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Quan hệ với Transactions
        public ICollection<Transaction>? Transactions { get; set; }

        // Quan hệ với Funds
        public ICollection<Fund>? Funds { get; set; }
    }
}