using Microsoft.EntityFrameworkCore;
using WebsiteQuanLyChiTieu.Data;
using WebsiteQuanLyChiTieu.Models;
using WebsiteQuanLyChiTieu.Repositories;

public class CategoryRepository : IRepository<Category>
{
    private readonly ApplicationDbContext _context;
    public CategoryRepository(ApplicationDbContext context) { _context = context; }

    public async Task<IEnumerable<Category>> GetAllAsync() => await _context.Categories.ToListAsync();
    public async Task<Category?> GetByIdAsync(int id) => await _context.Categories.FindAsync(id);
    public async Task AddAsync(Category entity) { _context.Categories.Add(entity); await _context.SaveChangesAsync(); }
    public async Task UpdateAsync(Category entity) { _context.Categories.Update(entity); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(int id) { var entity = await GetByIdAsync(id); if (entity != null) { _context.Categories.Remove(entity); await _context.SaveChangesAsync(); } }
}