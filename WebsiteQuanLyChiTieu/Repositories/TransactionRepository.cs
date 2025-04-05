using Microsoft.EntityFrameworkCore;
using WebsiteQuanLyChiTieu.Data;
using WebsiteQuanLyChiTieu.Models;
using WebsiteQuanLyChiTieu.Repositories;

public class TransactionRepository : IRepository<Transaction>
{
    private readonly ApplicationDbContext _context;
    public TransactionRepository(ApplicationDbContext context) { _context = context; }

    public async Task<IEnumerable<Transaction>> GetAllAsync() => await _context.Transactions.ToListAsync();
    public async Task<Transaction?> GetByIdAsync(int id) => await _context.Transactions.FindAsync(id);
    public async Task AddAsync(Transaction entity) { _context.Transactions.Add(entity); await _context.SaveChangesAsync(); }
    public async Task UpdateAsync(Transaction entity) { _context.Transactions.Update(entity); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var entity = await GetByIdAsync(id); if (entity != null) { _context.Transactions.Remove(entity); await _context.SaveChangesAsync(); } }

    public Task<Transaction?> GetByIdAsync(int key1, int key2) => throw new NotImplementedException();
    public Task DeleteAsync(int key1, int key2) => throw new NotImplementedException();
}
