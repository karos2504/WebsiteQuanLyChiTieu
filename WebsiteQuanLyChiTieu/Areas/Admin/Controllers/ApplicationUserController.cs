using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WebsiteQuanLyChiTieu.Areas.Admin.Repositories;
using WebsiteQuanLyChiTieu.Areas.Admin.Models;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ApplicationUserController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationUserController(IUserRepository userRepository, RoleManager<IdentityRole> roleManager)
    {
        _userRepository = userRepository;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userRepository.GetAllAsync();
        return View(users);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    public IActionResult Create()
    {
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApplicationUser user, string password, string role)
    {
        if (ModelState.IsValid)
        {
            var result = await _userRepository.CreateAsync(user, password);
            if (result)
            {
                await _userRepository.AssignRoleAsync(user, role);
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", "Failed to create user.");
        }
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(user);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, ApplicationUser user, string newRole)
    {
        if (id != user.Id) return NotFound();

        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null) return NotFound();

        existingUser.FullName = user.FullName;
        existingUser.Email = user.Email;

        var result = await _userRepository.UpdateAsync(existingUser);
        if (result)
        {
            await _userRepository.AssignRoleAsync(existingUser, newRole);
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Failed to update user.");
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(user);
    }

    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var result = await _userRepository.DeleteAsync(id);
        if (!result) return NotFound();

        return RedirectToAction(nameof(Index));
    }
}
