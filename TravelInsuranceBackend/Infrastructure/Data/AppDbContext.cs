using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    
    // EF Core uses this class to translate C# operations into SQL queries
    // Each DbSet below maps directly to a TABLE in SQL Server
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Each line below = one SQL table
        public DbSet<Policy> Policies => Set<Policy>();                          //  Policies table
        public DbSet<Domain.Entities.Claim> Claims => Set<Domain.Entities.Claim>(); // Claims table - OUR CLAIM IS SAVED HERE
        public DbSet<PolicyProduct> PolicyProducts => Set<PolicyProduct>();      // PolicyProducts table
        public DbSet<ClaimDocument> ClaimDocuments { get; set; }                 // ClaimDocuments table
        public DbSet<PolicyRequest> PolicyRequests { get; set; }                 // PolicyRequests table
        public DbSet<PolicyRequestDocument> PolicyRequestDocuments { get; set; } //PolicyRequestDocuments table
        public DbSet<Notification> Notifications { get; set; }                   //Notifications table
        public DbSet<CountryRisk> CountryRisks { get; set; }                     //CountryRisks table

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

            builder.Entity<PolicyRequest>(entity =>
            {
                entity.Property(e => e.CalculatedPremium)
                      .HasColumnType("decimal(18,2)");
                
                // Prevent cascade delete issues with multiple ApplicationUser foreign keys
                entity.HasOne(e => e.Customer)
                      .WithMany()
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Agent)
                      .WithMany()
                      .HasForeignKey(e => e.AgentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CountryRisk>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Multiplier).HasColumnType("decimal(18,2)");
            });
        }
    }
}