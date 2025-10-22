using CVAnalyzer.Core.Entities;
using CVAnalyzer.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CVAnalyzer.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Apply migrations
            await context.Database.MigrateAsync();

            // Seed roles
            await SeedRolesAsync(roleManager);

            // Seed default admin user
            await SeedAdminUserAsync(userManager);

            // Seed sample skills (optional)
            await SeedSkillsAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in UserRoles.GetAllRoles())
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@cvanalyzer.com";
            const string adminPassword = "Admin@123"; // Change this in production!

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    Department = "IT",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                }
            }
        }

        private static async Task SeedSkillsAsync(ApplicationDbContext context)
        {
            if (await context.Skills.AnyAsync())
                return;

            var skills = new List<Skill>
        {
            // Programming Languages
            new Skill { SkillName = "C#", NormalizedName = "CSHARP", Category = "Programming" },
            new Skill { SkillName = "Python", NormalizedName = "PYTHON", Category = "Programming" },
            new Skill { SkillName = "Java", NormalizedName = "JAVA", Category = "Programming" },
            new Skill { SkillName = "JavaScript", NormalizedName = "JAVASCRIPT", Category = "Programming" },
            new Skill { SkillName = "TypeScript", NormalizedName = "TYPESCRIPT", Category = "Programming" },
            
            // Frameworks
            new Skill { SkillName = "ASP.NET Core", NormalizedName = "ASPNETCORE", Category = "Framework" },
            new Skill { SkillName = "React", NormalizedName = "REACT", Category = "Framework" },
            new Skill { SkillName = "Angular", NormalizedName = "ANGULAR", Category = "Framework" },
            new Skill { SkillName = "Django", NormalizedName = "DJANGO", Category = "Framework" },
            
            // Databases
            new Skill { SkillName = "SQL Server", NormalizedName = "SQLSERVER", Category = "Database" },
            new Skill { SkillName = "MySQL", NormalizedName = "MYSQL", Category = "Database" },
            new Skill { SkillName = "PostgreSQL", NormalizedName = "POSTGRESQL", Category = "Database" },
            new Skill { SkillName = "MongoDB", NormalizedName = "MONGODB", Category = "Database" },
            
            // Cloud & DevOps
            new Skill { SkillName = "Azure", NormalizedName = "AZURE", Category = "Cloud" },
            new Skill { SkillName = "AWS", NormalizedName = "AWS", Category = "Cloud" },
            new Skill { SkillName = "Docker", NormalizedName = "DOCKER", Category = "DevOps" },
            new Skill { SkillName = "Kubernetes", NormalizedName = "KUBERNETES", Category = "DevOps" },
            
            // Soft Skills
            new Skill { SkillName = "Communication", NormalizedName = "COMMUNICATION", Category = "Soft Skill" },
            new Skill { SkillName = "Team Leadership", NormalizedName = "TEAMLEADERSHIP", Category = "Soft Skill" },
            new Skill { SkillName = "Problem Solving", NormalizedName = "PROBLEMSOLVING", Category = "Soft Skill" }
        };

            await context.Skills.AddRangeAsync(skills);
            await context.SaveChangesAsync();
        }
    }
}