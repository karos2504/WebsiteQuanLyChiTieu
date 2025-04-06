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
            var transactions = await _transactionRepository.GetAllAsync();
            return View(transactions);
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _transactionRepository.GetByIdAsync(id.Value);
            if (transaction == null) return NotFound();

            return View(transaction);
        }

        // GET: Transaction/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var funds = await _fundRepository.GetAllAsync();

            if (!categories.Any() || !funds.Any())
            {
                ModelState.AddModelError("", "Không có danh mục hoặc quỹ nào trong hệ thống. Vui lòng thêm trước khi tạo giao dịch.");
            }

            ViewData["CategoryID"] = new SelectList(categories, "CategoryID", "CategoryName");
            ViewData["FundID"] = new SelectList(funds, "FundID", "FundName");
            return View(new Transaction
            {
                CreatedById = _userManager.GetUserId(User),
                CreatedAt = DateTime.UtcNow
            });
        }


        // POST: Transaction/Create
        // POST: Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("CategoryID,FundID,Type,Amount,Description,CreatedById,Status,CreatedAt")] Transaction transaction)
        {
            transaction.CreatedById = _userManager.GetUserId(User);
            transaction.CreatedAt = DateTime.UtcNow;

            // Automatically approve if the transaction type is "Income"
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
                else
                {
                    await _transactionRepository.AddAsync(transaction);

                    // Update the fund amount if the transaction is approved
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

            ViewData["CategoryID"] = new SelectList(await _categoryRepository.GetAllAsync(), "CategoryID", "CategoryName", transaction.CategoryID);
            ViewData["FundID"] = new SelectList(await _fundRepository.GetAllAsync(), "FundID", "FundName", transaction.FundID);
            return View(transaction);
        }



        // GET: Transaction/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var transaction = await _transactionRepository.GetByIdAsync(id.Value);
            if (transaction == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && transaction.CreatedById != currentUserId)
            {
                return Forbid();
            }

            if (!isAdmin && transaction.Status != "Pending")
            {
                return Forbid();
            }

            return View(transaction);
        }

        // POST: Transaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionID,Status,Description")] Transaction updatedTransaction)
        {
            if (id != updatedTransaction.TransactionID) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");
            var existingTransaction = await _transactionRepository.GetByIdAsync(id);

            if (existingTransaction == null) return NotFound();

            if (!isAdmin && existingTransaction.CreatedById != currentUserId)
            {
                return Forbid();
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
                        // Admin thay đổi Status
                        var previousStatus = existingTransaction.Status;
                        existingTransaction.Status = updatedTransaction.Status;

                        if (existingTransaction.Status == "Approved" && previousStatus != "Approved")
                        {
                            existingTransaction.ApprovedById = currentUserId;
                            // Cập nhật Fund khi phê duyệt
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
                        // User chỉ thay đổi Description nếu Status là Pending
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
                else
                {
                    var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                    System.Diagnostics.Debug.WriteLine("ModelState Errors: " + string.Join(", ", errors));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating transaction: {ex.Message}");
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

            // Khi hủy, đặt Status thành Rejected
            transaction.Status = "Rejected";
            transaction.ApprovedById = null;
            await _transactionRepository.UpdateAsync(transaction);

            return RedirectToAction(nameof(Index));
        }
    }
}