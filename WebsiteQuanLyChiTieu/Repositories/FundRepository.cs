using Microsoft.EntityFrameworkCore;
using WebsiteQuanLyChiTieu.Data;
using WebsiteQuanLyChiTieu.Models;

namespace WebsiteQuanLyChiTieu.Repositories
{
    public class FundRepository : IRepository<Fund>
    {
        private readonly ApplicationDbContext _context;

        public FundRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Fund>> GetAllAsync()
        {
            return await _context.Funds.ToListAsync();
        }

        public async Task<Fund?> GetByIdAsync(int id)
        {
            return await _context.Funds.FindAsync(id);
        }

        public async Task AddAsync(Fund entity)
        {
            _context.Funds.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Fund entity)
        {
            _context.Funds.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Funds.FindAsync(id);
            if (entity != null)
            {
                _context.Funds.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public Task<Fund?> GetByIdAsync(int key1, int key2)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int key1, int key2)
        {
            throw new NotImplementedException();
        }
    }
}
