using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQuanLyChiTieu.Data;
using WebsiteQuanLyChiTieu.Models;

namespace WebsiteQuanLyChiTieu.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Report
        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports.ToListAsync();
            return View(reports);
        }

        // GET: Report/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var report = await _context.Reports
                .Include(r => r.Transactions)
                .FirstOrDefaultAsync(m => m.ReportID == id);
            if (report == null) return NotFound();

            // Tính toán TotalAmount từ Transactions (nếu cần)
            if (report.Transactions != null && report.Transactions.Any())
            {
                report.TotalAmount = report.Transactions.Sum(t => t.Amount);
            }

            return View(report);
        }
    }
}