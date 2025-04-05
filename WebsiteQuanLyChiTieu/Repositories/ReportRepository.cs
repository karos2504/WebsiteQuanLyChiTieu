using Microsoft.EntityFrameworkCore;
using WebsiteQuanLyChiTieu.Data;
using WebsiteQuanLyChiTieu.Models;
using WebsiteQuanLyChiTieu.Repositories;

public class ReportRepository : IRepository<Report>
{
    private readonly ApplicationDbContext _context;
    public ReportRepository(ApplicationDbContext context) { _context = context; }

    public async Task<IEnumerable<Report>> GetAllAsync() => await _context.Reports.ToListAsync();
    public async Task<Report?> GetByIdAsync(int id) => await _context.Reports.FindAsync(id);
    public async Task AddAsync(Report entity) { _context.Reports.Add(entity); await _context.SaveChangesAsync(); }
    public async Task UpdateAsync(Report entity) { _context.Reports.Update(entity); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var entity = await GetByIdAsync(id); if (entity != null) { _context.Reports.Remove(entity); await _context.SaveChangesAsync(); } }

    public Task<Report?> GetByIdAsync(int key1, int key2) => throw new NotImplementedException();
    public Task DeleteAsync(int key1, int key2) => throw new NotImplementedException();
}
