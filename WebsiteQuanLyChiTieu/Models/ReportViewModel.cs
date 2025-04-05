using System.Collections.Generic;

namespace WebsiteQuanLyChiTieu.Models.ViewModels
{
    public class ReportViewModel
    {
        public List<Transaction> Transactions { get; set; } = new();
    }
}
