namespace WebsiteQuanLyChiTieu.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        public string? CategoryName { get; set; }

        public string? Description { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }
    }
}