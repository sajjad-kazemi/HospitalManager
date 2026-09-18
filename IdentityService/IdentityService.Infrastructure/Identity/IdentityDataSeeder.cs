using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure.Identity
{
    public sealed class IdentityDataSeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        public async Task SeedAsync()
        {
            foreach (var roleName in HospitalRoles.All)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                    continue;

                var result = await roleManager.CreateAsync(
                new IdentityRole<Guid>(roleName));

                EnsureSucceeded(result, $"creating role '{roleName}'");
            }

            var email = configuration["BootstrapAdmin:Email"]
            ?? throw new InvalidOperationException(
                "Missing configuration: BootstrapAdmin:Email");

            var password = configuration["BootstrapAdmin:Password"]
                ?? throw new InvalidOperationException(
                    "Missing configuration: BootstrapAdmin:Password");

            var firstName = configuration["BootstrapAdmin:FirstName"]
                ?? throw new InvalidOperationException(
                    "Missing configuration: BootstrapAdmin:FirstName");

            var lastName = configuration["BootstrapAdmin:LastName"]
                ?? throw new InvalidOperationException(
                    "Missing configuration: BootstrapAdmin:LastName");

            var admin = await userManager.FindByEmailAsync(email);


            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var createResult = await userManager.CreateAsync(admin, password);
                EnsureSucceeded(createResult, "creating bootstrap Admin");
            }

            if (!await userManager.IsInRoleAsync(admin, HospitalRoles.Admin))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    admin,
                    HospitalRoles.Admin);

                EnsureSucceeded(roleResult, "assigning Admin role");
            }
        }
        private static void EnsureSucceeded(
        IdentityResult result,
        string operation)
        {
            if (result.Succeeded)
                return;

            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"Identity error while {operation}: {errors}");
        }
    }
}
