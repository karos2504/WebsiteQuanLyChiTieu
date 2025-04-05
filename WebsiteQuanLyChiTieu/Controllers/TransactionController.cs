using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebsiteQuanLyChiTieu.Data;
using WebsiteQuanLyChiTieu.Models;

namespace WebsiteQuanLyChiTieu.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var transactions = _context.Transactions
                .Include(t => t.Category)      // Bao gồm Category
                .Include(t => t.Fund)          // Bao gồm Fund
                .Include(t => t.CreatedBy)     // Bao gồm CreatedBy
                .Include(t => t.ApprovedBy)    // Bao gồm ApprovedBy
                .ToListAsync();

            return View(await transactions);
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Category)      // Bao gồm Category
                .Include(t => t.Fund)          // Bao gồm Fund
                .Include(t => t.CreatedBy)     // Bao gồm CreatedBy
                .Include(t => t.ApprovedBy)    // Bao gồm ApprovedBy
                .FirstOrDefaultAsync(m => m.TransactionID == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create()
        {
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            ViewData["FundID"] = new SelectList(_context.Funds, "FundID", "FundName");
            return View();
        }

        // POST: Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionID,Amount,Description,CategoryID,FundID,Type,Status,CreatedById")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                // Set the CreatedAt to current time and CreatedById to the logged-in user
                transaction.CreatedAt = DateTime.UtcNow;
                transaction.CreatedById = User.Identity.Name;

                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName", transaction.CategoryID);
            ViewData["FundID"] = new SelectList(_context.Funds, "FundID", "FundName", transaction.FundID);
            return View(transaction);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName", transaction.CategoryID);
            ViewData["FundID"] = new SelectList(_context.Funds, "FundID", "FundName", transaction.FundID);
            return View(transaction);
        }

        // POST: Transaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionID,Amount,Description,CategoryID,FundID,Type,Status,CreatedById,ApprovedById")] Transaction transaction)
        {
            if (id != transaction.TransactionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryName", transaction.CategoryID);
            ViewData["FundID"] = new SelectList(_context.Funds, "FundID", "FundName", transaction.FundID);
            return View(transaction);
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Fund)
                .Include(t => t.CreatedBy)
                .Include(t => t.ApprovedBy)
                .FirstOrDefaultAsync(m => m.TransactionID == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionID == id);
        }
    }
}
