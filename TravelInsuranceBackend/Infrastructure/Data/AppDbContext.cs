using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<Domain.Entities.Claim> Claims => Set<Domain.Entities.Claim>();
        public DbSet<PolicyProduct> PolicyProducts => Set<PolicyProduct>();
        public DbSet<ClaimDocument> ClaimDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Domain.Entities.Claim>(entity =>
            {
                entity.Property(e => e.ClaimedAmount)
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApprovedAmount)
                      .HasColumnType("decimal(18,2)");
            });

            builder.Entity<Policy>(entity =>
            {
                entity.Property(e => e.PremiumAmount)
                      .HasColumnType("decimal(18,2)");
            });

            builder.Entity<PolicyProduct>(entity =>
            {
                entity.Property(e => e.BasePremium)
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.CoverageLimit)
                      .HasColumnType("decimal(18,2)");
                entity.Property(e => e.ClaimLimit)
                      .HasColumnType("decimal(18,2)");
            });
        }
    }
}