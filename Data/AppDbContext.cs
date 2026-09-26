using Microsoft.EntityFrameworkCore;

namespace FormsApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected AppDbContext()
        {
        }

        public DbSet<Models.FormData> FormData { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.FormData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1024);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
            });
        }
    }
}
