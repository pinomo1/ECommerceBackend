using ECommerce1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce1.Services
{
    public class AccountDbContext : IdentityDbContext<AuthUser>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AccountDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>(e =>
            {
                e.HasKey(x => x.Token);
                e.Property(x => x.Token).IsRequired();
            });
        }
    }
}
