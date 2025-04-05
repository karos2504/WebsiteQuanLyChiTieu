using System.ComponentModel.DataAnnotations;

namespace WebsiteQuanLyChiTieu.Models
{
    public class Fund
    {
        public int FundID { get; set; }

        public string? FundName { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
    }
}