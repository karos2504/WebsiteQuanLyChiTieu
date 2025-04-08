using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteQuanLyChiTieu.Areas.Admin.Models;

namespace WebsiteQuanLyChiTieu.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        [Required]
        public string CategoryName { get; set; }

        public string? Description { get; set; }

        public string? UserID { get; set; } // Thêm trường UserID để liên kết với User

        [ForeignKey("UserID")]
        public ApplicationUser? User { get; set; } // Mối quan hệ với ApplicationUser

        public ICollection<Transaction>? Transactions { get; set; }
    }
}