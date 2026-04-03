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

            // and informal codes (like AU, AT) are removed from the Name field.
            var targetCountries = new List<CountryRisk>
            {
                new CountryRisk { Name = "Nepal", Multiplier = 1.1m },
                new CountryRisk { Name = "Sri Lanka", Multiplier = 1.1m },
                new CountryRisk { Name = "Bangladesh", Multiplier = 1.1m },

                // Zone 2: SE Asia & Middle East (1.15x - 1.25x)
                new CountryRisk { Name = "Thailand", Multiplier = 1.15m },
                new CountryRisk { Name = "Vietnam", Multiplier = 1.15m },
                new CountryRisk { Name = "Indonesia", Multiplier = 1.2m },
                new CountryRisk { Name = "Malaysia", Multiplier = 1.2m },
                new CountryRisk { Name = "Singapore", Multiplier = 1.2m },
                new CountryRisk { Name = "United Arab Emirates", Multiplier = 1.25m },
                new CountryRisk { Name = "Qatar", Multiplier = 1.25m },
                new CountryRisk { Name = "Turkey", Multiplier = 1.25m },

                // Zone 3: Schengen area, UK & East Asia (1.35x - 1.50x)
                new CountryRisk { Name = "United Kingdom", Multiplier = 1.45m },
                new CountryRisk { Name = "Germany", Multiplier = 1.4m },
                new CountryRisk { Name = "France", Multiplier = 1.4m },
                new CountryRisk { Name = "Italy", Multiplier = 1.4m },
                new CountryRisk { Name = "Spain", Multiplier = 1.4m },
                new CountryRisk { Name = "Netherlands", Multiplier = 1.4m },
                new CountryRisk { Name = "Greece", Multiplier = 1.4m },
                new CountryRisk { Name = "Portugal", Multiplier = 1.4m },
                new CountryRisk { Name = "Japan", Multiplier = 1.45m },
                new CountryRisk { Name = "South Korea", Multiplier = 1.45m },

                // Zone 4: High Cost / Wide Infrastructure (1.55x - 1.75x)
                new CountryRisk { Name = "Australia", Multiplier = 1.6m },
                new CountryRisk { Name = "New Zealand", Multiplier = 1.6m },
                new CountryRisk { Name = "Russia", Multiplier = 1.7m },
                new CountryRisk { Name = "Mexico", Multiplier = 1.55m },
                new CountryRisk { Name = "Brazil", Multiplier = 1.55m },
                new CountryRisk { Name = "South Africa", Multiplier = 1.6m },

                // Zone 5: Premium Medical Costs (1.80x - 2.1x)
                new CountryRisk { Name = "USA", Multiplier = 2.1m },
                new CountryRisk { Name = "Canada", Multiplier = 1.9m },
                new CountryRisk { Name = "Switzerland", Multiplier = 1.85m },
                new CountryRisk { Name = "Israel", Multiplier = 1.8m }
            };

            var currentInDb = await context.CountryRisks.ToListAsync();
            foreach (var target in targetCountries)
            {
                var existing = currentInDb.FirstOrDefault(c => 
                    c.Name.Equals(target.Name, StringComparison.OrdinalIgnoreCase) || 
                    c.Name.Contains(target.Name, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.Name = target.Name; 
                    existing.Multiplier = target.Multiplier;
                }
                else
                {
                    context.CountryRisks.Add(target);
                }
            }

            await context.SaveChangesAsync();

            // ── Seed WOW DATA for Dashboard ──────────────────
            if (!await context.PolicyRequests.AnyAsync())
            {
                var customerId = (await userManager.FindByEmailAsync(customerEmail))?.Id ?? "";
                var agentsList = await userManager.GetUsersInRoleAsync(UserRole.Agent);
                var productList = await context.PolicyProducts.ToListAsync();
                
                var agent1 = agentsList.FirstOrDefault(a => a.Email == "agent1@talktravel.com");
                var agent2 = agentsList.FirstOrDefault(a => a.Email == "agent2@talktravel.com");
                
                var silverSingle = productList.FirstOrDefault(p => p.PolicyName == "Silver Single Trip");
                var goldSingle = productList.FirstOrDefault(p => p.PolicyName == "Gold Single Trip");
                var platFamily = productList.FirstOrDefault(p => p.PolicyName == "Platinum Family");

                // 1. Seed Policy Requests
                var demoRequests = new List<PolicyRequest>
                {
                    new PolicyRequest { 
                        PolicyProductId = silverSingle!.PolicyProductId, CustomerId = customerId, AgentId = agent1?.Id,
                        Destination = "Thailand", StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow.AddDays(20),
                        TravellerName = "Anjali Sharma", TravellerAge = 28, PassportNumber = "L1234567", KycType = "Aadhaar", KycNumber = "123412341234",
                        Status = "Pending", RiskLevel = "Low", RequestedAt = DateTime.UtcNow.AddDays(-2), CalculatedPremium = 2875 
                    },
                    new PolicyRequest { 
                        PolicyProductId = goldSingle!.PolicyProductId, CustomerId = customerId, AgentId = agent2?.Id,
                        Destination = "USA", StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(15),
                        TravellerName = "Vikram Singh", TravellerAge = 45, PassportNumber = "M9876543", KycType = "PAN", KycNumber = "ABCDE1234F",
                        Status = "Approved", RiskLevel = "Medium", RequestedAt = DateTime.UtcNow.AddDays(-5), CalculatedPremium = 11550 
                    },
                    new PolicyRequest { 
                        PolicyProductId = platFamily!.PolicyProductId, CustomerId = customerId, AgentId = agent1?.Id,
                        Destination = "Switzerland", StartDate = DateTime.UtcNow.AddDays(30), EndDate = DateTime.UtcNow.AddDays(45),
                        TravellerName = "The Kapoors", TravellerAge = 35, PassportNumber = "P1122334", KycType = "Aadhaar", KycNumber = "999988887777",
                        Status = "Rejected", RejectionReason = "High Risk Area / Document Mismatch", RiskLevel = "High", RequestedAt = DateTime.UtcNow.AddDays(-1), CalculatedPremium = 33300 
                    }
                };
                await context.PolicyRequests.AddRangeAsync(demoRequests);

                // 2. Seed Active Policies (Revenue Drivers)
                var demoPolicies = new List<Policy>
                {
                    new Policy {
                        PolicyProductId = silverSingle.PolicyProductId, PolicyNumber = "POL-2026-0001", CustomerId = customerId, AgentId = agent1?.Id,
                        Destination = "Singapore", PolicyType = "Single Trip", PlanTier = "Silver", 
                        TravellerName = "Rohit Verma", TravellerAge = 30, PassportNumber = "N1122334", KycType = "Aadhaar", KycNumber = "777766665555",
                        PremiumAmount = 3000, StartDate = DateTime.UtcNow.AddDays(-5), EndDate = DateTime.UtcNow.AddDays(25),
                        Status = PolicyStatus.Active, CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new Policy {
                        PolicyProductId = goldSingle.PolicyProductId, PolicyNumber = "POL-2026-0002", CustomerId = customerId, AgentId = agent2?.Id,
                        Destination = "United Kingdom", PolicyType = "Single Trip", PlanTier = "Gold", 
                        TravellerName = "Sneha Iyer", TravellerAge = 34, PassportNumber = "K9988776", KycType = "PAN", KycNumber = "ZZZZZ9999P",
                        PremiumAmount = 7975, StartDate = DateTime.UtcNow.AddDays(-2), EndDate = DateTime.UtcNow.AddDays(28),
                        Status = PolicyStatus.Active, CreatedAt = DateTime.UtcNow.AddDays(-4)
                    },
                    new Policy {
                        PolicyProductId = platFamily.PolicyProductId, PolicyNumber = "POL-2026-0003", CustomerId = customerId, AgentId = agent1?.Id,
                        Destination = "USA", PolicyType = "Family", PlanTier = "Platinum", 
                        TravellerName = "Mehta Family", TravellerAge = 40, PassportNumber = "A0099887", KycType = "Aadhaar", KycNumber = "111100002222",
                        PremiumAmount = 37800, StartDate = DateTime.UtcNow.AddDays(-15), EndDate = DateTime.UtcNow.AddDays(15),
                        Status = PolicyStatus.Active, CreatedAt = DateTime.UtcNow.AddDays(-20)
                    }
                };
                await context.Policies.AddRangeAsync(demoPolicies);
                await context.SaveChangesAsync();

                // 3. Seed Claims (Analytics)
                var activePols = await context.Policies.ToListAsync();
                var demoClaims = new List<Claim>
                {
                    new Claim {
                        PolicyId = activePols[0].PolicyId, CustomerId = customerId, ClaimType = "Medical Emergency",
                        Description = "Food poisoning and hospital admission in Singapore", Status = ClaimStatus.UnderReview,
                        ClaimedAmount = 45000, SubmittedAt = DateTime.UtcNow.AddDays(-2), IncidentDate = DateTime.UtcNow.AddDays(-3)
                    },
                    new Claim {
                        PolicyId = activePols[1].PolicyId, CustomerId = customerId, ClaimType = "Loss of Passport",
                        Description = "Passport stolen at London Tube station", Status = ClaimStatus.Approved,
                        ClaimedAmount = 5000, ApprovedAmount = 5000, SubmittedAt = DateTime.UtcNow.AddDays(-25), 
                        ReviewedAt = DateTime.UtcNow.AddDays(-23), IncidentDate = DateTime.UtcNow.AddDays(-26)
                    }
                };
                await context.Claims.AddRangeAsync(demoClaims);
                await context.SaveChangesAsync();
            }
        }

    }
}
