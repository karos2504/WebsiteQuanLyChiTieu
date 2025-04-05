using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteQuanLyChiTieu.Models
{
    public class Report
    {
        [Key]
        public int ReportID { get; set; }

        [Required]
        public string ReportType { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Transaction>? Transactions { get; set; }
    }
}