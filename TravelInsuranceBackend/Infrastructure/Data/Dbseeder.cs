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
                // ── SINGLE TRIP ─────────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Single Trip",
                    PolicyType      = "Single Trip",
                    PlanTier        = "Silver",
                    BasePremium     = 1198,
                    CoverageLimit   = 4150000,
                    ClaimLimit      = 2075000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses (Upto Base Sum Insured, $100 deductible), Dental Expenses ($300 max, $150 deductible), Hospital Cash ($15/day max 5 days, 48hrs waiting), Personal Accident - $5000, Personal Accident Common Carrier - $5000",
                    ExclusionDetails = "Pre-existing Conditions, Self-inflicted Injuries, War or Civil Unrest, Alcohol or Drug Related Incidents, Cosmetic Surgery, Pregnancy Related Expenses",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Gold Single Trip",
                    PolicyType      = "Single Trip",
                    PlanTier        = "Gold",
                    BasePremium     = 1495,
                    CoverageLimit   = 8300000,
                    ClaimLimit      = 4150000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "All Silver Plan Benefits Included, Loss of Checked-in Baggage - $200, Delay of Checked-in Baggage - $250 (12hrs waiting), Theft of Baggage - $100 ($100 deductible), Flight Cancellation - $100, Trip Cancellation - $100 ($50 deductible), Loss of Passport - $200, Flight Delay - $100 (6hrs waiting), Trip Curtailment - $100 ($50 deductible), Emergency Hotel Accommodation - $1000 ($100 deductible)",
                    ExclusionDetails = "Pre-existing Conditions, Self-inflicted Injuries, War or Civil Unrest, Alcohol or Drug Related Incidents, Cosmetic Surgery, Pregnancy Related Expenses, Extreme Sports without Add-on",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Single Trip",
                    PolicyType      = "Single Trip",
                    PlanTier        = "Platinum",
                    BasePremium     = 2063,
                    CoverageLimit   = 41500000,
                    ClaimLimit      = 20750000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "All Gold Plan Benefits Included, Pre-Existing Disease Coverage (3% of base up to $10000), Loss of International Driving License, No Claim Discount, Personal Liability - $10000, Missed Flight Connection - $500, Hijack Distress Allowance - $100/day, Trip Delay - $100, Emergency Cash Assistance - $500",
                    ExclusionDetails = "Self-inflicted Injuries, War or Civil Unrest, Alcohol or Drug Related Incidents, Cosmetic Surgery, Pregnancy Related Expenses",
                    CreatedAt       = DateTime.UtcNow
                },

                // ── MULTI TRIP ──────────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Multi Trip",
                    PolicyType      = "Multi-Trip",
                    PlanTier        = "Silver",
                    BasePremium     = 2189,
                    CoverageLimit   = 4150000,
                    ClaimLimit      = 2075000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses (Upto Base Sum Insured, $100 deductible), Dental Expenses ($300 max, $150 deductible), Hospital Cash ($15/day max 5 days, 48hrs waiting), Personal Accident - $5000, Personal Accident Common Carrier - $5000, Unlimited Trips Per Year (Max 30 days per trip)",
                    ExclusionDetails = "Pre-existing Conditions, Self-inflicted Injuries, War or Civil Unrest, Alcohol or Drug Related Incidents, Cosmetic Surgery, Pregnancy Related Expenses",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Gold Multi Trip",
                    PolicyType      = "Multi-Trip",
                    PlanTier        = "Gold",
                    BasePremium     = 2730,
                    CoverageLimit   = 8300000,
                    ClaimLimit      = 4150000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "All Silver Plan Benefits Included, Loss of Checked-in Baggage - $200, Delay of Checked-in Baggage - $250 (12hrs waiting), Theft of Baggage - $100, Flight Cancellation - $100, Trip Cancellation - $100, Loss of Passport - $200, Flight Delay - $100 (6hrs waiting), Emergency Hotel Accommodation - $1000, Unlimited Trips Per Year (Max 30 days per trip)",
                    ExclusionDetails = "Pre-existing Conditions, Self-inflicted Injuries, War or Civil Unrest, Alcohol or Drug Related Incidents, Cosmetic Surgery, Pregnancy Related Expenses",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Multi Trip",
                    PolicyType      = "Multi-Trip",
                    PlanTier        = "Platinum",
                    BasePremium     = 3768,
                    CoverageLimit   = 41500000,
                    ClaimLimit      = 20750000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "All Gold Plan Benefits Included, Pre-Existing Disease Coverage (3% of base up to $10000), Personal Liability - $10000, Missed Flight Connection - $500, Hijack Distress Allowance - $100/day, Trip Delay - $100, Emergency Cash Assistance - $500, No Claim Discount, Unlimited Trips Per Year (Max 30 days per trip)",
                    ExclusionDetails = "Self-inflicted Injuries, War or Civil Unrest, Alcohol or Drug Related Incidents, Cosmetic Surgery, Pregnancy Related Expenses",
                    CreatedAt       = DateTime.UtcNow
                },

                // ── FAMILY PLANS ────────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Family",
                    PolicyType      = "Family",
                    PlanTier        = "Silver",
                    BasePremium     = 1198,
                    CoverageLimit   = 4150000,
                    ClaimLimit      = 2075000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses (Upto Base Sum Insured, $100 deductible), Dental Expenses ($300 max, $150 deductible), Hospital Cash ($15/day max 5 days, 48hrs waiting), Personal Accident - $5000 per person, Personal Accident Common Carrier - $5000, Covers up to 6 members (Max 2 Adults up to 60 yrs + Children up to 21 yrs)",
                    ExclusionDetails = "Pre-existing Conditions, Self-inflicted Injuries, War or Civil Unrest, Cosmetic Surgery, Pregnancy Related Expenses",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Gold Family",
                    PolicyType      = "Family",
                    PlanTier        = "Gold",
                    BasePremium     = 1495,
                    CoverageLimit   = 8300000,
                    ClaimLimit      = 4150000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "All Silver Plan Benefits Included, Loss of Checked-in Baggage - $200, Delay of Checked-in Baggage - $250, Theft of Baggage - $100, Flight Cancellation - $100, Trip Cancellation - $100, Loss of Passport - $200, Flight Delay - $100 (6hrs waiting), Emergency Hotel Accommodation - $1000, Covers up to 6 members (Max 2 Adults up to 60 yrs + Children up to 21 yrs)",
                    ExclusionDetails = "Pre-existing Conditions, Self-inflicted Injuries, War or Civil Unrest, Cosmetic Surgery, Pregnancy Related Expenses",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Family",
                    PolicyType      = "Family",
                    PlanTier        = "Platinum",
                    BasePremium     = 2063,
                    CoverageLimit   = 41500000,
                    ClaimLimit      = 20750000,
                    Tenure          = 30,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "All Gold Plan Benefits Included, Pre-Existing Disease Coverage, Personal Liability - $10000, Missed Flight Connection - $500, Hijack Distress Allowance, Trip Delay - $100, Emergency Cash Assistance - $500, Covers up to 6 members (Max 2 Adults up to 60 yrs + Children up to 21 yrs)",
                    ExclusionDetails = "Self-inflicted Injuries, War or Civil Unrest, Cosmetic Surgery, Pregnancy Related Expenses",
                    CreatedAt       = DateTime.UtcNow
                },

                // ── STUDENT PLANS ───────────────────────────────────
                new PolicyProduct
                {
                    PolicyName      = "Silver Student",
                    PolicyType      = "Student",
                    PlanTier        = "Silver",
                    BasePremium     = 11500,
                    CoverageLimit   = 8300000,
                    ClaimLimit      = 4150000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "Emergency Medical Expenses (Upto Base Sum Insured, $100 deductible), Dental Expenses ($300, $150 deductible), Medical Evacuation and Repatriation, Study Interruption, Sponsor Protection, Bail Bond, Compassionate Visit, Loss and Delay of Checked-in Baggage, Loss of Passport and Documents, Personal Accident - $5000, Age: 16-35 years only",
                    ExclusionDetails = "Pre-existing Conditions, Self-inflicted Injuries, War or Civil Unrest, Alcohol or Drug Related Incidents, Cosmetic Surgery, Pregnancy Related Expenses",
                    CreatedAt       = DateTime.UtcNow
                },
                new PolicyProduct
                {
                    PolicyName      = "Platinum Student",
                    PolicyType      = "Student",
                    PlanTier        = "Platinum",
                    BasePremium     = 24800,
                    CoverageLimit   = 41500000,
                    ClaimLimit      = 20750000,
                    Tenure          = 365,
                    DestinationZone = "Worldwide",
                    Status          = PolicyProductStatus.Available,
                    CoverageDetails = "All Silver Student Benefits Included, High Medical Coverage - $500000, University Requirements Coverage, Mental Health Coverage, Personal Liability - $10000, Hijack Distress Allowance, COVID-19 Coverage, Emergency Cash Assistance - $500, Age: 16-35 years only",
                    ExclusionDetails = "Self-inflicted Injuries, War or Civil Unrest, Alcohol or Drug Related Incidents, Cosmetic Surgery, Pregnancy Related Expenses",
                    CreatedAt       = DateTime.UtcNow
                }
            };

            await context.PolicyProducts.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        public static object SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            throw new NotImplementedException();
        }
    }
}
