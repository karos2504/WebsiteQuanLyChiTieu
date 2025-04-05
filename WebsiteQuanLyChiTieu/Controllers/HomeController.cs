using Microsoft.AspNetCore.Mvc;
using WebsiteQuanLyChiTieu.Models;
using WebsiteQuanLyChiTieu.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace WebsiteQuanLyChiTieu.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Fund> _fundRepository;

        public HomeController(
            IRepository<Transaction> transactionRepository,
            IRepository<Category> categoryRepository,
            IRepository<Fund> fundRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _fundRepository = fundRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // 1. Tổng Thu Nhập, Tổng Chi Tiêu, Số Dư
                var transactions = await _transactionRepository.GetAllAsync();
                var totalIncome = transactions.Where(t => t.Type == "Income" && t.Status == "Approved").Sum(t => t.Amount);
                var totalExpense = transactions.Where(t => t.Type == "Expense" && t.Status == "Approved").Sum(t => t.Amount);
                var balance = totalIncome - totalExpense;
                // Nếu thu nhập < chi tiêu, đặt số dư = 0
                if (balance < 0) balance = 0;

                ViewBag.TotalIncome = totalIncome.ToString("C0");
                ViewBag.TotalExpend = totalExpense.ToString("C0");
                ViewBag.Balance = balance.ToString("C0");

                // 2. Dữ liệu cho biểu đồ bánh (Chi Tiêu Theo Danh Mục)
                var categories = await _categoryRepository.GetAllAsync();
                var expenseByCategory = transactions
                    .Where(t => t.Type == "Expense" && t.Status == "Approved")
                    .GroupBy(t => t.CategoryID)
                    .Select(g => new
                    {
                        CategoryID = g.Key,
                        Amount = g.Sum(t => t.Amount)
                    }).ToList();

                var doughnutChartData = expenseByCategory.Select(ec => new DoughnutChartData
                {
                    CategoryTitleWithIcon = categories.FirstOrDefault(c => c.CategoryID == ec.CategoryID)?.CategoryName ?? "Unknown",
                    Amount = ec.Amount,
                    FormattedAmount = ec.Amount.ToString("C0")
                }).ToList();
                ViewBag.DoughnutChartData = doughnutChartData;

                // 3. Dữ liệu cho biểu đồ đường (Thu Nhập vs Chi Tiêu theo ngày)
                var splineChartData = transactions
                    .Where(t => t.Status == "Approved")
                    .GroupBy(t => t.CreatedAt.Date)
                    .Select(g => new SplineChartData
                    {
                        Day = g.Key.ToString("dd/MM/yyyy"),
                        Income = g.Where(t => t.Type == "Income").Sum(t => t.Amount),
                        Expense = g.Where(t => t.Type == "Expense").Sum(t => t.Amount)
                    }).OrderBy(x => DateTime.Parse(x.Day)).ToList();
                ViewBag.SplineChartData = splineChartData;

                // 4. Giao dịch gần đây (5 giao dịch mới nhất)
                var recentTransactions = transactions
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .Select(t => new RecentTransactionData
                    {
                        CategoryTitleWithIcon = categories.FirstOrDefault(c => c.CategoryID == t.CategoryID)?.CategoryName ?? "Unknown",
                        Date = t.CreatedAt,
                        FormattedAmount = t.Type == "Income" ? $"+{t.Amount:C0}" : $"-{t.Amount:C0}"
                    }).ToList();
                ViewBag.RecentTransactions = recentTransactions;
            }
            else
            {
                // Khi chưa đăng nhập, để ViewBag trống
                ViewBag.TotalIncome = null;
                ViewBag.TotalExpend = null;
                ViewBag.Balance = null;
                ViewBag.DoughnutChartData = null;
                ViewBag.SplineChartData = null;
                ViewBag.RecentTransactions = null;
            }

            return View();
        }
    }
}