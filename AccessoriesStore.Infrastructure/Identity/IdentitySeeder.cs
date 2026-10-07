using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
                string[] roles =
                {
                    "Admin",
                    "Customer"
                };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            const string adminEmail = "admin@accessoriesstore.com";

            var adminPassword = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Admin seed password is not configured.");
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Store",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    admin,
                    adminPassword);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
