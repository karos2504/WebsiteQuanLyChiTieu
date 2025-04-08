using Microsoft.AspNetCore.Mvc;
using WebsiteQuanLyChiTieu.Models;
using WebsiteQuanLyChiTieu.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using WebsiteQuanLyChiTieu.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebsiteQuanLyChiTieu.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Fund> _fundRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IRepository<Transaction> transactionRepository,
            IRepository<Category> categoryRepository,
            IRepository<Fund> fundRepository,
            UserManager<ApplicationUser> userManager)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _fundRepository = fundRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string selectedUserId = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                SetEmptyData();
                return View();
            }

            var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if (isAdmin)
            {
                // Admin: Hiển thị dropdown chọn User
                var allUsers = await _userManager.Users.ToListAsync();
                ViewBag.AllUsers = allUsers;

                if (!string.IsNullOrEmpty(selectedUserId))
                {
                    var selectedUser = await _userManager.FindByIdAsync(selectedUserId);
                    if (selectedUser != null)
                    {
                        ViewBag.SelectedUserFullName = selectedUser.FullName;
                        await LoadUserFundData(selectedUserId);
                    }
                    else
                    {
                        SetEmptyData();
                    }
                }
                else
                {
                    SetEmptyData();
                }
            }
            else
            {
                // User thường: Chỉ hiển thị dữ liệu cá nhân
                ViewBag.SelectedUserFullName = currentUser.FullName;
                await LoadUserFundData(currentUserId);
            }

            return View();
        }

        // Phương thức tải dữ liệu từ quỹ của User
        private async Task LoadUserFundData(string userId)
        {
            var funds = await _fundRepository.GetAllAsync();
            var userFund = funds.FirstOrDefault(f => f.UserID == userId);

            if (userFund != null)
            {
                var transactions = await _transactionRepository.GetAllAsync();
                var userTransactions = transactions.Where(t => t.FundID == userFund.FundID).ToList();
                await LoadDashboardData(userTransactions);
            }
            else
            {
                SetEmptyData();
            }
        }

        // Phương thức tính toán và gán dữ liệu cho ViewBag
        private async Task LoadDashboardData(List<Transaction> transactions)
        {
            var totalIncome = transactions.Where(t => t.Type == "Income" && t.Status == "Approved").Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Type == "Expense" && t.Status == "Approved").Sum(t => t.Amount);
            var balance = totalIncome - totalExpense;
            if (balance < 0) balance = 0;

            ViewBag.TotalIncome = totalIncome.ToString("C0");
            ViewBag.TotalExpend = totalExpense.ToString("C0");
            ViewBag.Balance = balance.ToString("C0");

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

        // Phương thức gán dữ liệu rỗng
        private void SetEmptyData()
        {
            ViewBag.TotalIncome = null;
            ViewBag.TotalExpend = null;
            ViewBag.Balance = null;
            ViewBag.DoughnutChartData = null;
            ViewBag.SplineChartData = null;
            ViewBag.RecentTransactions = null;
            ViewBag.SelectedUserFullName = null;
        }
    }
}