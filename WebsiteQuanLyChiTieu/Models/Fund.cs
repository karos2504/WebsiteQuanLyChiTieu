using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteQuanLyChiTieu.Areas.Admin.Models;

namespace WebsiteQuanLyChiTieu.Models
{
    public class Fund
    {
        public int FundID { get; set; }

        public string? FundName { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public string? UserID { get; set; }

        [ForeignKey("UserID")]
        public ApplicationUser? User { get; set; } // Người được cấp quỹ

        public ICollection<Transaction>? Transactions { get; set; }
    }
}