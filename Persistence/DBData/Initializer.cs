using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence.DBContext;
using Persistence.DBModels;
using Persistence.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DBData
{
    public class Initializer
    {
        public static async Task Init(ApplicationDbContext context, RoleManager<Role> roleManager, UserManager<Member> memberManager, IWebHostEnvironment environment)
        {
            await InsertRolesAndRules(roleManager, memberManager, context);
        }

        private static async Task InsertRolesAndRules(
            RoleManager<Role> roleManager,
            UserManager<Member> memberManager,
            ApplicationDbContext context)
        {
            // 1. Seed Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                var role = new Role("Admin");
                await roleManager.CreateAsync(role);
            }

            if (!await roleManager.RoleExistsAsync("Employer"))
            {
                var role = new Role("Employer");
                await roleManager.CreateAsync(role);
            }

            if (!await roleManager.RoleExistsAsync("Member"))
            {
                var role = new Role("Member");
                await roleManager.CreateAsync(role);
            }

            // 2. Seed Eligibility Rules
            if (!context.EligibilityRules.Any())
            {
                var pensionRules = new List<EligibilityRule>
                    {
                        new EligibilityRule
                        {
                            RuleName = "MinimumAge",
                            Description = "Member must be at least minimum pension age",
                            BenefitType = BenefitType.Retirement,
                            ThresholdValue = 55,
                            EvaluationOrder = 1,
                            ErrorCode = "AGE01",
                            CreatedDate = DateTime.UtcNow,
                            Status = Status.Active
                        },
                        new EligibilityRule
                        {
                            RuleName = "MinimumContributions",
                            Description = "Minimum 240 monthly contributions",
                            BenefitType = BenefitType.Retirement,
                            ThresholdValue = 240,
                            EvaluationOrder = 2,
                            ErrorCode = "CONT01",
                            CreatedDate = DateTime.UtcNow,
                            Status = Status.Active
                        },
                        new EligibilityRule
                        {
                            RuleName = "AccountActive",
                            Description = "Member account must be active",
                            BenefitType = BenefitType.Retirement,
                            IsBooleanRule = true,
                            EvaluationOrder = 3,
                            ErrorCode = "ACT01",
                            CreatedDate = DateTime.UtcNow,
                            Status = Status.Active
                        }
                    };

                await context.EligibilityRules.AddRangeAsync(pensionRules);
            }

            // 3. Seed Admin User
            var adminMemberExist = await memberManager.FindByEmailAsync("admin@nlpc.com");

            if (adminMemberExist == null)
            {
                try
                {
                    var adminMember = new Member
                    {
                        UserName = "PCMS",
                        FirstName = "NLPC",
                        LastName = "Pension",
                        NickName = "Admin",
                        ProfilePicture = "default-profile.png",
                        Email = "admin@nlpc.com",
                        EmailConfirmed = true,
                        PhoneNumber = "1234567890",
                        PhoneNumberConfirmed = true,
                        DateOfBirth = new DateTime(1985, 1, 1),
                        NationalIdentificationNumber = "1234567890",
                        Address = "Lagos, Nigeria",
                        BankAccountNumber = "0123456789",
                        BankName = "First Bank",
                        RefreshToken = Guid.NewGuid().ToString(),
                        RefreshTokenEndDate = DateTime.UtcNow.AddMonths(1),
                        EmployerId = null,
                        IsActive = true,
                        Status = Status.Active,
                        CreatedDate = DateTime.UtcNow,
                        MembershipType = MembershipType.Individual,
                        IsEligibleForBenefits = false,
                        Accounts = new HashSet<Account>()
                    };

                    string adminPassword = "Admin239074106*";

                    var createAdminMemberResult = await memberManager.CreateAsync(adminMember, adminPassword);

                    if (createAdminMemberResult.Succeeded)
                    {
                        await memberManager.AddToRoleAsync(adminMember, "Admin");

                        Admin admin = new Admin
                        {
                            MemberId = adminMember.Id,
                            CreatedDate = DateTime.UtcNow,
                            Status = Status.Active
                        };

                        context.Admins.Add(admin);
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception here
                    throw;
                }
            }

            // Save all changes at once
            await context.SaveChangesAsync();
        }
    }
}
