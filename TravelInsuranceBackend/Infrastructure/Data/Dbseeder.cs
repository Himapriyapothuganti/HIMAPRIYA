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
            if (await context.PolicyProducts.AnyAsync())
            {
                context.PolicyProducts.RemoveRange(context.PolicyProducts);
                await context.SaveChangesAsync();
            }

            var products = new List<PolicyProduct>
            {
                // ── SINGLE TRIP PLANS ──────────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Single Trip",
                    PolicyType      = "Single Trip",
                    PlanTier        = "Silver",
                    BasePremium     = 1200,
                    CoverageLimit   = 2500000, 
                    ClaimLimit      = 2500000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident",
                    ExclusionDetails = "Travel Claim,Pre-existing Conditions,Baggage Loss or Delay,Flight Cancellation or Delay,Trip Cancellation,Loss of Passport,Personal Liability,War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Extreme Sports,Alcohol or Drug Related Incidents",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Gold Single Trip",
                    PolicyType      = "Single Trip",
                    PlanTier        = "Gold",
                    BasePremium     = 2300,
                    CoverageLimit   = 7500000, 
                    ClaimLimit      = 7500000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident,Travel Claim",
                    ExclusionDetails = "Pre-existing Conditions,Personal Liability,Missed Flight Connection,Hijack Distress Allowance,Emergency Cash Assistance,War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Extreme Sports,Alcohol or Drug Related Incidents",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Single Trip",
                    PolicyType      = "Single Trip",
                    PlanTier        = "Platinum",
                    BasePremium     = 4500,
                    CoverageLimit   = 20000000, 
                    ClaimLimit      = 20000000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident,Travel Claim",
                    ExclusionDetails = "War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Intentional Criminal Acts,Nuclear or Radiation Hazards,Alcohol or Drug Related Incidents",
                    CreatedAt       = DateTime.UtcNow
                },

                // ── MULTI TRIP PLANS ──────────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Multi Trip",
                    PolicyType      = "Multi-Trip",
                    PlanTier        = "Silver",
                    BasePremium     = 3200,
                    CoverageLimit   = 2500000,
                    ClaimLimit      = 2500000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident",
                    ExclusionDetails = "Travel Claim,Pre-existing Conditions,Baggage Loss or Delay,Flight Cancellation or Delay,Trip Cancellation,Loss of Passport,Personal Liability,War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Extreme Sports,Alcohol or Drug Related Incidents,Trips Exceeding 30 Days",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Gold Multi Trip",
                    PolicyType      = "Multi-Trip",
                    PlanTier        = "Gold",
                    BasePremium     = 5700,
                    CoverageLimit   = 7500000,
                    ClaimLimit      = 7500000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident,Travel Claim",
                    ExclusionDetails = "Pre-existing Conditions,Personal Liability,Missed Flight Connection,Hijack Distress Allowance,Emergency Cash Assistance,War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Extreme Sports,Alcohol or Drug Related Incidents,Trips Exceeding 30 Days",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Multi Trip",
                    PolicyType      = "Multi-Trip",
                    PlanTier        = "Platinum",
                    BasePremium     = 11500,
                    CoverageLimit   = 20000000,
                    ClaimLimit      = 20000000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident,Travel Claim",
                    ExclusionDetails = "War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Intentional Criminal Acts,Nuclear or Radiation Hazards,Alcohol or Drug Related Incidents,Trips Exceeding 30 Days",
                    CreatedAt       = DateTime.UtcNow
                },

                // ── FAMILY PLANS ────────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Family",
                    PolicyType      = "Family",
                    PlanTier        = "Silver",
                    BasePremium     = 1200,
                    CoverageLimit   = 2500000,
                    ClaimLimit      = 2500000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident",
                    ExclusionDetails = "Travel Claim,Pre-existing Conditions,Baggage Loss or Delay,Flight Cancellation or Delay,Trip Cancellation,Loss of Passport,Personal Liability,War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Extreme Sports,Alcohol or Drug Related Incidents,Members Above Age 60,Children Above Age 21",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Gold Family",
                    PolicyType      = "Family",
                    PlanTier        = "Gold",
                    BasePremium     = 2200,
                    CoverageLimit   = 7500000,
                    ClaimLimit      = 7500000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident,Travel Claim",
                    ExclusionDetails = "Pre-existing Conditions,Personal Liability,Missed Flight Connection,Hijack Distress Allowance,Emergency Cash Assistance,War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Extreme Sports,Alcohol or Drug Related Incidents,Members Above Age 60,Children Above Age 21",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Family",
                    PolicyType      = "Family",
                    PlanTier        = "Platinum",
                    BasePremium     = 4500,
                    CoverageLimit   = 20000000,
                    ClaimLimit      = 20000000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident,Travel Claim",
                    ExclusionDetails = "War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Intentional Criminal Acts,Nuclear or Radiation Hazards,Alcohol or Drug Related Incidents,Members Above Age 60,Children Above Age 21",
                    CreatedAt       = DateTime.UtcNow
                },

                // ── STUDENT PLANS ───────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Student",
                    PolicyType      = "Student",
                    PlanTier        = "Silver",
                    BasePremium     = 20000,
                    CoverageLimit   = 7500000,
                    ClaimLimit      = 7500000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident,Travel Claim,Study Related Claim",
                    ExclusionDetails = "Pre-existing Conditions,University Requirements Coverage,Mental Health Coverage,Personal Liability,Hijack Distress Allowance,War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Alcohol or Drug Related Incidents,Students Below Age 16 Not Eligible,Students Above Age 35 Not Eligible",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Student",
                    PolicyType      = "Student",
                    PlanTier        = "Platinum",
                    BasePremium     = 55000,
                    CoverageLimit   = 20000000,
                    ClaimLimit      = 20000000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Medical Claim,Personal Accident,Travel Claim,Study Related Claim",
                    ExclusionDetails = "War or Civil Unrest,Self-inflicted Injuries,Cosmetic Surgery,Pregnancy and Childbirth,Intentional Criminal Acts,Alcohol or Drug Related Incidents,Students Below Age 16 Not Eligible,Students Above Age 35 Not Eligible",
                    CreatedAt       = DateTime.UtcNow
                }
            };

            await context.PolicyProducts.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

    }
}
