using Microsoft.AspNetCore.Mvc;
using WebsiteQuanLyChiTieu.Models;
using WebsiteQuanLyChiTieu.Repositories;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WebsiteQuanLyChiTieu.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebsiteQuanLyChiTieu.Controllers
{
    public class FundController : Controller
    {
        private readonly IRepository<Fund> _fundRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public FundController(
            IRepository<Fund> fundRepository,
            UserManager<ApplicationUser> userManager)
        {
            _fundRepository = fundRepository;
            _userManager = userManager;
        }

        // READ: Hiển thị danh sách quỹ (cho cả Admin và User)
        [Authorize] // Cả Admin và User đều truy cập được
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var funds = await _fundRepository.GetAllAsync();
            if (!isAdmin)
            {
                // Nếu là User, chỉ hiển thị quỹ được cấp cho họ
                funds = funds.Where(f => f.UserID == currentUserId).ToList();
            }

            return View(funds);
        }

        // READ: Hiển thị chi tiết quỹ (cho cả Admin và User)
        [Authorize] // Cả Admin và User đều truy cập được
        public async Task<IActionResult> Details(int id)
        {
            var fund = await _fundRepository.GetByIdAsync(id);
            if (fund == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Nếu là User, chỉ cho xem quỹ của họ
            if (!isAdmin && fund.UserID != currentUserId)
            {
                return Forbid(); // Trả về lỗi 403 nếu User cố xem quỹ không thuộc về họ
            }

            return View(fund);
        }

        // CREATE: Hiển thị form tạo quỹ mới (chỉ Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // CREATE: Xử lý tạo quỹ mới (chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Fund fund)
        {
            if (ModelState.IsValid)
            {
                await _fundRepository.AddAsync(fund);
                return RedirectToAction(nameof(Index));
            }
            return View(fund);
        }

        // UPDATE: Hiển thị form chỉnh sửa quỹ (chỉ Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var fund = await _fundRepository.GetByIdAsync(id);
            if (fund == null)
            {
                return NotFound();
            }
            return View(fund);
        }

        // UPDATE: Xử lý chỉnh sửa quỹ (chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Fund fund)
        {
            if (id != fund.FundID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _fundRepository.UpdateAsync(fund);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "Không thể cập nhật quỹ. Vui lòng thử lại.");
                }
            }
            return View(fund);
        }

        // DELETE: Hiển thị xác nhận xóa quỹ (chỉ Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var fund = await _fundRepository.GetByIdAsync(id);
            if (fund == null)
            {
                return NotFound();
            }
            return View(fund);
        }

        // DELETE: Xử lý xóa quỹ (chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _fundRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ASSIGN: Hiển thị form cấp quỹ (chỉ Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignFundView()
        {
            var funds = await _fundRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Funds = funds;
            ViewBag.Users = users;
            return View();
        }

        // ASSIGN: Xử lý cấp quỹ cho User (chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignFund(int fundId, string userId)
        {
            var fund = await _fundRepository.GetByIdAsync(fundId);
            var user = await _userManager.FindByIdAsync(userId);

            if (fund != null && user != null)
            {
                if (fund.UserID != null && fund.UserID != userId)
                {
                    ViewBag.ErrorMessage = "Quỹ này đã được cấp cho người dùng khác.";
                }
                else
                {
                    fund.UserID = userId; // Gán quỹ cho User
                    await _fundRepository.UpdateAsync(fund);
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Không tìm thấy quỹ hoặc người dùng.";
            }

            var funds = await _fundRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Funds = funds;
            ViewBag.Users = users;
            return View("AssignFundView");
        }
    }
}