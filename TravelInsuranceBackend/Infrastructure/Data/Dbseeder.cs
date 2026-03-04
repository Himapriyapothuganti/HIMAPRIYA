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

            // ── Seed Agent ─────────────────────────────────
            const string agentEmail = "agent@talktravel.com";
            const string agentPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(
                agentEmail) == null)
            {
                var agent = new ApplicationUser
                {
                    FullName = "Test Agent",
                    Email = agentEmail,
                    UserName = agentEmail,
                    Role = UserRole.Agent,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager
                    .CreateAsync(agent, agentPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(
                        agent, UserRole.Agent);
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
            if (!await context.PolicyProducts.AnyAsync())
            {
                var products = new List<PolicyProduct>
                {
                    // ── SINGLE TRIP ───────────────────────
                    new PolicyProduct
                    {
                        PolicyName      = "Silver Single Trip",
                        PolicyType      = "Single Trip",
                        PlanTier        = "Silver",
                        CoverageDetails = "Medical Expenses & Hospitalization, Emergency Medical Evacuation, Repatriation of Remains, Trip Cancellation, Personal Accident, Loss of Passport",
                        CoverageLimit   = 500000,
                        ClaimLimit      = 250000,
                        BasePremium     = 299,
                        Tenure          = 30,
                        DestinationZone = "Asia",
                        Status          = PolicyProductStatus.Available,
                        CreatedAt       = DateTime.UtcNow
                    },
                    new PolicyProduct
                    {
                        PolicyName      = "Gold Single Trip",
                        PolicyType      = "Single Trip",
                        PlanTier        = "Gold",
                        CoverageDetails = "Medical Expenses & Hospitalization, Emergency Medical Evacuation, Repatriation of Remains, Trip Cancellation & Interruption, Personal Accident, Loss of Passport, Baggage Loss & Delay, Flight Delay (>4hrs), Missed Flight Connection, Emergency Dental Treatment, Hijack Distress Allowance",
                        CoverageLimit   = 1000000,
                        ClaimLimit      = 500000,
                        BasePremium     = 499,
                        Tenure          = 30,
                        DestinationZone = "Asia,Europe,Schengen",
                        Status          = PolicyProductStatus.Available,
                        CreatedAt       = DateTime.UtcNow
                    },
                    new PolicyProduct
                    {
                        PolicyName      = "Platinum Single Trip",
                        PolicyType      = "Single Trip",
                        PlanTier        = "Platinum",
                        CoverageDetails = "Medical Expenses & Hospitalization, Emergency Medical Evacuation, Repatriation of Remains, Trip Cancellation & Interruption, Personal Accident, Loss of Passport, Baggage Loss & Delay, Flight Delay (>2hrs), Missed Flight Connection, Emergency Dental Treatment, Hijack Distress Allowance, Adventure Sports Coverage, Pre-existing Conditions, COVID-19 Coverage, Daily Hospital Cash Allowance, Emergency Hotel Extension",
                        CoverageLimit   = 2500000,
                        ClaimLimit      = 1250000,
                        BasePremium     = 999,
                        Tenure          = 30,
                        DestinationZone = "Worldwide",
                        Status          = PolicyProductStatus.Available,
                        CreatedAt       = DateTime.UtcNow
                    },

                    // ── MULTI TRIP ────────────────────────
                    new PolicyProduct
                    {
                        PolicyName      = "Silver Multi Trip",
                        PolicyType      = "Multi-Trip",
                        PlanTier        = "Silver",
                        CoverageDetails = "Medical Expenses & Hospitalization, Emergency Medical Evacuation, Repatriation of Remains, Trip Cancellation, Personal Accident, Loss of Passport, Unlimited Trips Per Year",
                        CoverageLimit   = 500000,
                        ClaimLimit      = 250000,
                        BasePremium     = 2999,
                        Tenure          = 365,
                        DestinationZone = "Asia",
                        Status          = PolicyProductStatus.Available,
                        CreatedAt       = DateTime.UtcNow
                    },
                    new PolicyProduct
                    {
                        PolicyName      = "Gold Multi Trip",
                        PolicyType      = "Multi-Trip",
                        PlanTier        = "Gold",
                        CoverageDetails = "Medical Expenses & Hospitalization, Emergency Medical Evacuation, Repatriation of Remains, Trip Cancellation & Interruption, Personal Accident, Loss of Passport, Baggage Loss & Delay, Flight Delay (>4hrs), Missed Flight Connection, Emergency Dental Treatment, Hijack Distress Allowance, Unlimited Trips Per Year",
                        CoverageLimit   = 1000000,
                        ClaimLimit      = 500000,
                        BasePremium     = 4999,
                        Tenure          = 365,
                        DestinationZone = "Asia,Europe,Schengen",
                        Status          = PolicyProductStatus.Available,
                        CreatedAt       = DateTime.UtcNow
                    },
                    new PolicyProduct
                    {
                        PolicyName      = "Platinum Multi Trip",
                        PolicyType      = "Multi-Trip",
                        PlanTier        = "Platinum",
                        CoverageDetails = "Medical Expenses & Hospitalization, Emergency Medical Evacuation, Repatriation of Remains, Trip Cancellation & Interruption, Personal Accident, Loss of Passport, Baggage Loss & Delay, Flight Delay (>2hrs), Missed Flight Connection, Emergency Dental Treatment, Hijack Distress Allowance, Adventure Sports Coverage, Pre-existing Conditions, COVID-19 Coverage, Daily Hospital Cash Allowance, Unlimited Trips Per Year",
                        CoverageLimit   = 2500000,
                        ClaimLimit      = 1250000,
                        BasePremium     = 7999,
                        Tenure          = 365,
                        DestinationZone = "Worldwide",
                        Status          = PolicyProductStatus.Available,
                        CreatedAt       = DateTime.UtcNow
                    },

                    // ── FAMILY ────────────────────────────
                    new PolicyProduct
                    {
                        PolicyName      = "Silver Family",
                        PolicyType      = "Family",
                        PlanTier        = "Silver",
                        CoverageDetails = "Medical Expenses & Hospitalization, Emergency Medical Evacuation, Repatriation of Remains, Trip Cancellation, Personal Accident, Loss of Passport (2 Adults + 2 Children)",
                        CoverageLimit   = 500000,
                        ClaimLimit      = 250000,
                        BasePremium     = 999,
                        Tenure          = 30,
                        DestinationZone = "Asia",
                        Status          = PolicyProductStatus.Available,
                        CreatedAt       = DateTime.UtcNow
                    },
                    new PolicyProduct
                    {
                        PolicyName      = "Gold Family",
                        PolicyType      = "Family",
                        PlanTier        = "Gold",
                        CoverageDetails = "Medical Expenses & Hospitalization, Emergency Medical Evacuation, Repatriation of Remains, Trip Cancellation & Interruption, Personal Accident, Loss of Passport, Baggage Loss & Delay, Flight Delay (>4hrs), Missed Flight Connection, Emergency Dental Treatment (2 Adults + 2 Children)",
                        CoverageLimit   = 1000000,
                        ClaimLimit      = 500000,
                        BasePremium     = 1999,
                        Tenure          = 30,
                        DestinationZone = "Asia,Europe,Schengen",
                        Status          = PolicyProductStatus.Available,
                        CreatedAt       = DateTime.UtcNow
                    },
                    new PolicyProduct
                    {
                        PolicyName      = "Platinum Family",
                        PolicyType      = "Family",
                        PlanTier        = "Platinum",
                        CoverageDetails = "Medical Expenses & Hospitalization, Emergency Medical Evacuation, Repatriation of Remains, Trip Cancellation & Interruption, Personal Accident, Loss of Passport, Baggage Loss & Delay, Flight Delay (>2hrs), Missed Flight Connection, Emergency Dental Treatment, Adventure Sports Coverage, Pre-existing Conditions, COVID-19 Coverage (2 Adults + 2 Children)",
                        CoverageLimit   = 2500000,
                        ClaimLimit      = 1250000,
                        BasePremium     = 3499,
                        Tenure          = 30,
                        DestinationZone = "Worldwide",
                        Status          = PolicyProductStatus.Available,
                        CreatedAt       = DateTime.UtcNow
                    },

                    // ── STUDENT ───────────────────────────
                    new PolicyProduct
                    {
                        PolicyName      = "Platinum Student",
                        PolicyType      = "Student",
                        PlanTier        = "Platinum",
                        CoverageDetails = "Medical Expenses & Hospitalization, Emergency Medical Evacuation, Repatriation of Remains, Study Interruption, Sponsor Protection, Bail Bond, Baggage Loss & Delay, Personal Accident, Pre-existing Conditions, COVID-19 Coverage, University Requirements Coverage (Age 16-35)",
                        CoverageLimit   = 5000000,
                        ClaimLimit      = 2500000,
                        BasePremium     = 15000,
                        Tenure          = 365,
                        DestinationZone = "Worldwide",
                        Status          = PolicyProductStatus.Available,
                        CreatedAt       = DateTime.UtcNow
                    }
                };

                await context.PolicyProducts.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }

        public static object SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            throw new NotImplementedException();
        }
    }
}
