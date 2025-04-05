using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteQuanLyChiTieu.Areas.Admin.Models;
using WebsiteQuanLyChiTieu.Models;

namespace WebsiteQuanLyChiTieu.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Fund> Funds { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Transaction - CreatedBy (người tạo)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.CreatedBy)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction - Fund (1-n)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Fund)
                .WithMany(f => f.Transactions)
                .HasForeignKey(t => t.FundID)
                .OnDelete(DeleteBehavior.Cascade);

            // Transaction - ApprovedBy (nếu có)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ApprovedBy)
                .WithMany()
                .HasForeignKey(t => t.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction - Category
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryID)
                .OnDelete(DeleteBehavior.SetNull); // hoặc DeleteBehavior.Cascade nếu cần
        }
    }
}
