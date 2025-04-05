using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebsiteQuanLyChiTieu.Areas.Admin.Models;

namespace WebsiteQuanLyChiTieu.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }

        [Required]
        public string CreatedById { get; set; } = string.Empty; // Người tạo giao dịch

        [ForeignKey("CreatedById")]
        public ApplicationUser? CreatedBy { get; set; }

        // Mối quan hệ với Category
        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }

        // Mối quan hệ với Fund
        public int FundID { get; set; }

        [ForeignKey("FundID")]
        public Fund? Fund { get; set; }  // Liên kết với Fund

        [Required]
        public string Type { get; set; } = string.Empty; // "Income" / "Expense"

        [Required]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string Status { get; set; } = "Pending"; // "Pending", "Approved"

        public string? ApprovedById { get; set; }

        [ForeignKey("ApprovedById")]
        public ApplicationUser? ApprovedBy { get; set; }
    }
}

