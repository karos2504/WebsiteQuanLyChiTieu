using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebsiteQuanLyChiTieu.Data;
using WebsiteQuanLyChiTieu.Models;

namespace WebsiteQuanLyChiTieu.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    // Cập nhật constructor để inject cả ApplicationDbContext
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        // Tính tổng thu nhập (Income) từ bảng Transactions
        var totalIncome = _context.Transactions
                               .Where(t => t.Status == "Approved" && t.Type == "Income") // Lọc giao dịch đã approved và loại Income
                               .Sum(t => t.Amount);

        ViewBag.totalIncome = totalIncome.ToString("N0"); // Gán vào ViewBag (đổi tên thành totalIncome)

        // Tính tổng chi tiêu (Expend) từ bảng Transactions
        var totalExpend = _context.Transactions
                               .Where(t => t.Status == "Approved" && t.Type == "Expend") // Lọc giao dịch đã approved và loại Expend
                               .Sum(t => t.Amount);

        ViewBag.TotalExpend = totalExpend.ToString("N0"); // Gán vào ViewBag (sửa từ totalIncome thành totalExpend)

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}