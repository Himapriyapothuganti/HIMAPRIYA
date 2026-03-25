using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public static class Dbseeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            // ── Seed Roles ─────────────────────────────────
            string[] roles = [
                UserRole.Admin,
                UserRole.Agent,
                UserRole.Customer,
                UserRole.ClaimsOfficer
            ];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
            }

            // ── Seed Admin ─────────────────────────────────
            const string adminEmail = "admin@gmail.com";
            const string adminPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(
                adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    FullName = "System Admin",
                    Email = adminEmail,
                    UserName = adminEmail,
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager
                    .CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(
                        admin, UserRole.Admin);
            }

            // ── Seed Agents ────────────────────────────────
            var agentSeeds = new[]
            {
                new { Email = "agent1@talktravel.com", Name = "Rahul Sharma" },
                new { Email = "agent2@talktravel.com", Name = "Priya Nair" },
                new { Email = "agent3@talktravel.com", Name = "Arjun Mehta" }
            };

            foreach (var agentSeed in agentSeeds)
            {
                if (await userManager.FindByEmailAsync(agentSeed.Email) == null)
                {
                    var agent = new ApplicationUser
                    {
                        FullName = agentSeed.Name,
                        Email = agentSeed.Email,
                        UserName = agentSeed.Email,
                        Role = UserRole.Agent,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    var result = await userManager
                        .CreateAsync(agent, "Admin@123");
                    if (result.Succeeded)
                        await userManager.AddToRoleAsync(
                            agent, UserRole.Agent);
                }
            }

            // ── Seed Customer ──────────────────────────────
            const string customerEmail = "customer@talktravel.com";
            const string customerPassword = "Customer@123";

            if (await userManager.FindByEmailAsync(
                customerEmail) == null)
            {
                var customer = new ApplicationUser
                {
                    FullName = "Test Customer",
                    Email = customerEmail,
                    UserName = customerEmail,
                    Role = UserRole.Customer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager
                    .CreateAsync(customer, customerPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(
                        customer, UserRole.Customer);
            }

            // ── Seed Claims Officer ────────────────────────
            const string officerEmail = "officer@talktravel.com";
            const string officerPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(
                officerEmail) == null)
            {
                var officer = new ApplicationUser
                {
                    FullName = "Claims Officer",
                    Email = officerEmail,
                    UserName = officerEmail,
                    Role = UserRole.ClaimsOfficer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager
                    .CreateAsync(officer, officerPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(
                        officer, UserRole.ClaimsOfficer);
            }

            // ── Seed Policy Products ───────────────────────
            var existingProducts = await context.PolicyProducts.ToDictionaryAsync(p => p.PolicyName);

            var products = new List<PolicyProduct>
            {
                // ── SINGLE TRIP PLANS ──────────────────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Single Trip",
                    PolicyType      = "Single Trip",
                    PlanTier        = "Silver",
                    BasePremium     = 2500,
                    CoverageLimit   = 2500000,
                    ClaimLimit      = 2500000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Loss of Passport",
                    ExclusionDetails = "Pre-existing Diseases, Adventure Sports, Pregnancy",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Gold Single Trip",
                    PolicyType      = "Single Trip",
                    PlanTier        = "Gold",
                    BasePremium     = 5500,
                    CoverageLimit   = 7500000,
                    ClaimLimit      = 7500000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Trip Cancellation, Baggage Loss, Flight Delay, Loss of Passport",
                    ExclusionDetails = "Pre-existing Diseases, Adventure Sports, Pregnancy",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Single Trip",
                    PolicyType      = "Single Trip",
                    PlanTier        = "Platinum",
                    BasePremium     = 12500,
                    CoverageLimit   = 20000000,
                    ClaimLimit      = 20000000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Trip Cancellation, Baggage Loss, Flight Delay, Medical Evacuation, Personal Liability, Loss of Passport",
                    ExclusionDetails = "Pre-existing Diseases, Adventure Sports, Pregnancy",
                    CreatedAt       = DateTime.UtcNow
                },

                // ── FAMILY PLANS ───────────────────────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Family",
                    PolicyType      = "Family",
                    PlanTier        = "Silver",
                    BasePremium     = 4500,
                    CoverageLimit   = 2500000,
                    ClaimLimit      = 2500000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Loss of Passport",
                    ExclusionDetails = "Pre-existing Diseases, Adventure Sports, Pregnancy, Members Above 60",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Gold Family",
                    PolicyType      = "Family",
                    PlanTier        = "Gold",
                    BasePremium     = 8500,
                    CoverageLimit   = 7500000,
                    ClaimLimit      = 7500000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Trip Cancellation, Baggage Loss, Flight Delay, Loss of Passport",
                    ExclusionDetails = "Pre-existing Diseases, Adventure Sports, Pregnancy, Members Above 65",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Family",
                    PolicyType      = "Family",
                    PlanTier        = "Platinum",
                    BasePremium     = 18000,
                    CoverageLimit   = 20000000,
                    ClaimLimit      = 20000000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Trip Cancellation, Baggage Loss, Flight Delay, Medical Evacuation, Personal Liability, Loss of Passport",
                    ExclusionDetails = "Pre-existing Diseases, Adventure Sports, Pregnancy, Members Above 70",
                    CreatedAt       = DateTime.UtcNow
                },

                // ── STUDENT PLANS ─────────────────────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Student",
                    PolicyType      = "Student",
                    PlanTier        = "Silver",
                    BasePremium     = 35000,
                    CoverageLimit   = 7500000,
                    ClaimLimit      = 7500000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Tuition Protection",
                    ExclusionDetails = "Pre-existing Conditions, Pregnancy, Students Below 16 or Above 35",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Student",
                    PolicyType      = "Student",
                    PlanTier        = "Platinum",
                    BasePremium     = 85000,
                    CoverageLimit   = 20000000,
                    ClaimLimit      = 20000000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Tuition Protection, Baggage Loss, Trip Cancellation, Medical Evacuation",
                    ExclusionDetails = "Pre-existing Conditions, Pregnancy, Students Below 16 or Above 40",
                    CreatedAt       = DateTime.UtcNow
                },

                // ── MULTI-TRIP PLANS ──────────────────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Multi Trip",
                    PolicyType      = "Multi-Trip",
                    PlanTier        = "Silver",
                    BasePremium     = 8500,
                    CoverageLimit   = 2500000,
                    ClaimLimit      = 2500000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Loss of Passport",
                    ExclusionDetails = "Pre-existing Diseases, Adventure Sports, Trips > 30 Days",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Gold Multi Trip",
                    PolicyType      = "Multi-Trip",
                    PlanTier        = "Gold",
                    BasePremium     = 15000,
                    CoverageLimit   = 7500000,
                    ClaimLimit      = 7500000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Trip Cancellation, Baggage Loss, Flight Delay, Loss of Passport",
                    ExclusionDetails = "Pre-existing Diseases, Adventure Sports, Trips > 45 Days",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Multi Trip",
                    PolicyType      = "Multi-Trip",
                    PlanTier        = "Platinum",
                    BasePremium     = 32000,
                    CoverageLimit   = 20000000,
                    ClaimLimit      = 20000000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses, Trip Cancellation, Baggage Loss, Flight Delay, Medical Evacuation, Personal Liability, Loss of Passport",
                    ExclusionDetails = "Pre-existing Diseases, Adventure Sports, Trips > 60 Days",
                    CreatedAt       = DateTime.UtcNow
                }
            };

            foreach (var p in products)
            {
                if (existingProducts.TryGetValue(p.PolicyName, out var existing))
                {
                    // Update existing
                    existing.BasePremium = p.BasePremium;
                    existing.CoverageLimit = p.CoverageLimit;
                    existing.ClaimLimit = p.ClaimLimit;
                    existing.CoverageDetails = p.CoverageDetails;
                    existing.ExclusionDetails = p.ExclusionDetails;
                    existing.DestinationZone = p.DestinationZone;
                    existing.PlanTier = p.PlanTier;
                    existing.Tenure = p.Tenure;
                    existing.Status = p.Status;
                }
                else
                {
                    // Add new
                    await context.PolicyProducts.AddAsync(p);
                }
            }

            await context.SaveChangesAsync();
        }

    }
}
