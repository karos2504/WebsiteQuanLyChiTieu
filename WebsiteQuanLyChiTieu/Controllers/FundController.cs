﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteQuanLyChiTieu.Data;
using WebsiteQuanLyChiTieu.Models;

namespace WebsiteQuanLyChiTieu.Controllers
{
    public class FundController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FundController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Fund
        public async Task<IActionResult> Index()
        {
            return View(await _context.Funds.ToListAsync());
        }

        // GET: Fund/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var fund = await _context.Funds
                .FirstOrDefaultAsync(m => m.FundID == id);
            if (fund == null) return NotFound();

            return View(fund);
        }

        // GET: Fund/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Fund/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FundID,FundName,Amount,Description")] Fund fund)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fund);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fund);
        }

        // GET: Fund/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var fund = await _context.Funds.FindAsync(id);
            if (fund == null) return NotFound();
            return View(fund);
        }

        // POST: Fund/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FundID,FundName,Amount,Description")] Fund fund)
        {
            if (id != fund.FundID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fund);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FundExists(fund.FundID)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fund);
        }

        // GET: Fund/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var fund = await _context.Funds
                .FirstOrDefaultAsync(m => m.FundID == id);
            if (fund == null) return NotFound();

            return View(fund);
        }

        // POST: Fund/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fund = await _context.Funds.FindAsync(id);
            if (fund != null)
            {
                _context.Funds.Remove(fund);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FundExists(int id)
        {
            return _context.Funds.Any(e => e.FundID == id);
        }
    }
}