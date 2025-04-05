using Microsoft.EntityFrameworkCore;
using WebsiteQuanLyChiTieu.Data;
using WebsiteQuanLyChiTieu.Models;
using WebsiteQuanLyChiTieu.Repositories;

public class FundRepository : IRepository<Fund>
{
    private readonly ApplicationDbContext _context;

    public FundRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Fund>> GetAllAsync() => await _context.Funds.ToListAsync();

    public async Task<Fund?> GetByIdAsync(int id) => await _context.Funds.FindAsync(id);

    public async Task AddAsync(Fund entity)
    {
        _context.Funds.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Fund entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.Funds.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}