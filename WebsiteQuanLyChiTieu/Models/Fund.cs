using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteQuanLyChiTieu.Models
{
    public class Fund
    {
        public int FundID { get; set; }  // ID Quỹ

        public string? FundName { get; set; }  // Tên Quỹ

        public decimal Amount { get; set; }  // Số tiền trong quỹ

        public string? Description { get; set; }  // Mô tả về quỹ

        // Mối quan hệ với Transaction
        public ICollection<Transaction>? Transactions { get; set; }  // Các giao dịch liên quan đến quỹ

        
    }
}
