using Microsoft.EntityFrameworkCore;
using WebsiteQuanLyChiTieu.Data;
using WebsiteQuanLyChiTieu.Models;
using WebsiteQuanLyChiTieu.Repositories;

public class TransactionRepository : IRepository<Transaction>
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync() => await _context.Transactions
        .Include(t => t.Category)
        .Include(t => t.Fund)
        .Include(t => t.CreatedBy)
        .Include(t => t.ApprovedBy)
        .ToListAsync();

    public async Task<Transaction?> GetByIdAsync(int id) => await _context.Transactions
        .Include(t => t.Category)
        .Include(t => t.Fund)
        .Include(t => t.CreatedBy)
        .Include(t => t.ApprovedBy)
        .FirstOrDefaultAsync(t => t.TransactionID == id);

    public async Task AddAsync(Transaction entity)
    {
        _context.Transactions.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transaction entity)
    {
        try
        {
            _context.Entry(entity).State = EntityState.Modified;
            System.Diagnostics.Debug.WriteLine($"Saving changes for Transaction: {entity.TransactionID}");
            int rowsAffected = await _context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"Rows affected: {rowsAffected}");
            if (rowsAffected == 0)
            {
                throw new Exception("No rows were updated.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in UpdateAsync: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Transactions.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}