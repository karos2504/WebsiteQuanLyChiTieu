using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQuanLyChiTieu.Areas.Admin.Models;
using WebsiteQuanLyChiTieu.Models;
using WebsiteQuanLyChiTieu.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebsiteQuanLyChiTieu.Controllers
{
    [Authorize] // Yêu cầu đăng nhập
    public class TransactionController : Controller
    {
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Fund> _fundRepository;

        public TransactionController(
            IRepository<Transaction> transactionRepository,
            UserManager<ApplicationUser> userManager,
            IRepository<Category> categoryRepository,
            IRepository<Fund> fundRepository)
        {
            _transactionRepository = transactionRepository;
            _userManager = userManager;
            _categoryRepository = categoryRepository;
            _fundRepository = fundRepository;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            IEnumerable<Transaction> transactions = await _transactionRepository.GetAllAsync();

            if (!isAdmin)
            {
                // Lấy danh sách quỹ được cấp cho User
                var userFunds = await _fundRepository.GetAllAsync();
                var userFundIds = userFunds.Where(f => f.UserID == currentUser.Id).Select(f => f.FundID).ToList();

                // Chỉ hiển thị giao dịch thuộc quỹ của User
                transactions = transactions.Where(t => userFundIds.Contains(t.FundID));
            }

            return View(transactions);
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _transactionRepository.GetByIdAsync(id.Value);
            if (transaction == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if (!isAdmin)
            {
                var userFunds = await _fundRepository.GetAllAsync();
                var userFundIds = userFunds.Where(f => f.UserID == currentUser.Id).Select(f => f.FundID).ToList();

                if (!userFundIds.Contains(transaction.FundID))
                {
                    return Forbid(); // User không được xem giao dịch từ quỹ không thuộc về họ
                }
            }

            return View(transaction);
        }

        // GET: Transaction/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            // Lấy danh mục của User hoặc tất cả nếu là Admin
            var categories = await _categoryRepository.GetAllAsync();
            if (!isAdmin)
            {
                categories = categories.Where(c => c.UserID == currentUser.Id);
            }

            // Lấy quỹ: Admin thấy tất cả, User chỉ thấy quỹ được cấp
            var funds = await _fundRepository.GetAllAsync();
            if (!isAdmin)
            {
                funds = funds.Where(f => f.UserID == currentUser.Id);
            }

            if (!categories.Any() || !funds.Any())
            {
                ModelState.AddModelError("", "Không có danh mục hoặc quỹ nào trong hệ thống. Vui lòng thêm trước khi tạo giao dịch.");
            }

            ViewData["CategoryID"] = new SelectList(categories, "CategoryID", "CategoryName");
            ViewData["FundID"] = new SelectList(funds, "FundID", "FundName");
            return View(new Transaction
            {
                CreatedById = currentUser.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        // POST: Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryID,FundID,Type,Amount,Description,CreatedById,Status,CreatedAt")] Transaction transaction)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            transaction.CreatedById = currentUser.Id;
            transaction.CreatedAt = DateTime.UtcNow;

            // Tự động phê duyệt nếu là "Income"
            if (transaction.Type == "Income")
            {
                transaction.Status = "Approved";
            }
            else
            {
                transaction.Status = "Pending";
            }

            if (ModelState.IsValid)
            {
                var categoryExists = await _categoryRepository.GetByIdAsync(transaction.CategoryID) != null;
                var fundExists = await _fundRepository.GetByIdAsync(transaction.FundID) != null;
                var userExists = await _userManager.FindByIdAsync(transaction.CreatedById) != null;

                if (!categoryExists || !fundExists || !userExists)
                {
                    ModelState.AddModelError("", "Danh mục, quỹ hoặc người dùng không tồn tại.");
                }
                else if (!isAdmin)
                {
                    // Kiểm tra quyền truy cập quỹ và danh mục
                    var userFunds = await _fundRepository.GetAllAsync();
                    var userFundIds = userFunds.Where(f => f.UserID == currentUser.Id).Select(f => f.FundID).ToList();
                    var userCategories = await _categoryRepository.GetAllAsync();
                    var userCategoryIds = userCategories.Where(c => c.UserID == currentUser.Id).Select(c => c.CategoryID).ToList();

                    if (!userFundIds.Contains(transaction.FundID) || !userCategoryIds.Contains(transaction.CategoryID))
                    {
                        ModelState.AddModelError("", "Bạn chỉ có thể sử dụng quỹ và danh mục được cấp cho mình.");
                    }
                    else
                    {
                        await _transactionRepository.AddAsync(transaction);

                        // Cập nhật số tiền quỹ nếu được phê duyệt
                        if (transaction.Status == "Approved")
                        {
                            var fund = await _fundRepository.GetByIdAsync(transaction.FundID);
                            if (fund != null)
                            {
                                if (transaction.Type == "Income")
                                {
                                    fund.Amount += transaction.Amount;
                                }
                                else if (transaction.Type == "Expense")
                                {
                                    fund.Amount -= transaction.Amount;
                                }
                                await _fundRepository.UpdateAsync(fund);
                            }
                        }

                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    // Admin có thể dùng bất kỳ quỹ và danh mục nào
                    await _transactionRepository.AddAsync(transaction);

                    if (transaction.Status == "Approved")
                    {
                        var fund = await _fundRepository.GetByIdAsync(transaction.FundID);
                        if (fund != null)
                        {
                            if (transaction.Type == "Income")
                            {
                                fund.Amount += transaction.Amount;
                            }
                            else if (transaction.Type == "Expense")
                            {
                                fund.Amount -= transaction.Amount;
                            }
                            await _fundRepository.UpdateAsync(fund);
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            var categories = await _categoryRepository.GetAllAsync();
            var funds = await _fundRepository.GetAllAsync();
            if (!isAdmin)
            {
                categories = categories.Where(c => c.UserID == currentUser.Id);
                funds = funds.Where(f => f.UserID == currentUser.Id);
            }

            ViewData["CategoryID"] = new SelectList(categories, "CategoryID", "CategoryName", transaction.CategoryID);
            ViewData["FundID"] = new SelectList(funds, "FundID", "FundName", transaction.FundID);
            return View(transaction);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _transactionRepository.GetByIdAsync(id.Value);
            if (transaction == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if (!isAdmin)
            {
                var userFunds = await _fundRepository.GetAllAsync();
                var userFundIds = userFunds.Where(f => f.UserID == currentUser.Id).Select(f => f.FundID).ToList();

                if (!userFundIds.Contains(transaction.FundID) || transaction.CreatedById != currentUser.Id)
                {
                    return Forbid(); // User không được chỉnh sửa giao dịch từ quỹ không thuộc về họ
                }
                if (transaction.Status != "Pending")
                {
                    return Forbid(); // User chỉ chỉnh sửa giao dịch "Pending"
                }
            }

            return View(transaction);
        }

        // POST: Transaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionID,Status,Description")] Transaction updatedTransaction)
        {
            if (id != updatedTransaction.TransactionID) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);

            if (existingTransaction == null) return NotFound();

            if (!isAdmin)
            {
                var userFunds = await _fundRepository.GetAllAsync();
                var userFundIds = userFunds.Where(f => f.UserID == currentUser.Id).Select(f => f.FundID).ToList();

                if (!userFundIds.Contains(existingTransaction.FundID) || existingTransaction.CreatedById != currentUser.Id)
                {
                    return Forbid();
                }
            }

            try
            {
                // Xóa validation cho các trường không gửi từ form
                ModelState.Remove("CreatedById");
                ModelState.Remove("Type");
                ModelState.Remove("Amount");
                ModelState.Remove("CategoryID");
                ModelState.Remove("FundID");

                if (ModelState.IsValid)
                {
                    if (isAdmin)
                    {
                        var previousStatus = existingTransaction.Status;
                        existingTransaction.Status = updatedTransaction.Status;

                        if (existingTransaction.Status == "Approved" && previousStatus != "Approved")
                        {
                            existingTransaction.ApprovedById = currentUser.Id;
                            var fund = await _fundRepository.GetByIdAsync(existingTransaction.FundID);
                            if (fund != null)
                            {
                                if (existingTransaction.Type == "Income")
                                {
                                    fund.Amount += existingTransaction.Amount;
                                }
                                else if (existingTransaction.Type == "Expense")
                                {
                                    fund.Amount -= existingTransaction.Amount;
                                }
                                await _fundRepository.UpdateAsync(fund);
                            }
                        }
                        else if (existingTransaction.Status == "Rejected")
                        {
                            existingTransaction.ApprovedById = null;
                        }
                    }
                    else
                    {
                        if (existingTransaction.Status == "Pending")
                        {
                            existingTransaction.Description = updatedTransaction.Description;
                        }
                        else
                        {
                            return Forbid();
                        }
                    }

                    await _transactionRepository.UpdateAsync(existingTransaction);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra khi lưu: {ex.Message}");
            }

            return View(existingTransaction);
        }

        // POST: Transaction/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null) return NotFound();

            transaction.Status = "Rejected";
            transaction.ApprovedById = null;
            await _transactionRepository.UpdateAsync(transaction);

            return RedirectToAction(nameof(Index));
        }
    }
}