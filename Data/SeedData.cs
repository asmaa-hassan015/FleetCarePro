using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using FleetCarePro.Models;

namespace FleetCarePro.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var configuration =
                serviceProvider.GetRequiredService<IConfiguration>();

            // Roles required by FleetCare Pro
            string[] roles =
            {
                "Admin",
                "FleetManager",
                "Driver"
            };

            // =========================
            // Seed Roles
            // =========================

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult =
                        await roleManager.CreateAsync(
                            new IdentityRole(role));

                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(
                            ", ",
                            roleResult.Errors.Select(e => e.Description));

                        throw new Exception(
                            $"Failed to create role '{role}': {errors}");
                    }
                }
            }

            // =========================
            // Seed Admin
            // =========================

            await CreateUserFromConfiguration(
                userManager,
                configuration,
                "SeedAdmin",
                "Admin");

            // =========================
            // Seed Fleet Manager
            // =========================

            await CreateUserFromConfiguration(
                userManager,
                configuration,
                "SeedManager",
                "FleetManager");

            // =========================
            // Seed Driver
            // =========================

            await CreateUserFromConfiguration(
                userManager,
                configuration,
                "SeedDriver",
                "Driver");
        }


        private static async Task CreateUserFromConfiguration(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            string sectionName,
            string role)
        {
            var section =
                configuration.GetSection(sectionName);

            var email =
                section["Email"];

            var password =
                section["Password"];

            var fullName =
                section["FullName"];

            var employeeId =
                section["EmployeeId"];


            // =========================
            // Validate Configuration
            // =========================

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(employeeId))
            {
                return;
            }


            // =========================
            // Check Existing User
            // =========================

            var existingUser =
                await userManager.FindByEmailAsync(email);


            // =========================
            // Create User
            // =========================

            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmployeeId = employeeId,
                    EmailConfirmed = true
                };


                var result =
                    await userManager.CreateAsync(
                        user,
                        password);


                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"Failed to create user '{email}': {errors}");
                }


                // =========================
                // Assign Role
                // =========================

                var roleResult =
                    await userManager.AddToRoleAsync(
                        user,
                        role);


                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"Failed to assign role '{role}' to user '{email}': {errors}");
                }
            }
            else
            {
                // =========================
                // Ensure Existing User Has Role
                // =========================

                if (!await userManager.IsInRoleAsync(
                        existingUser,
                        role))
                {
                    var roleResult =
                        await userManager.AddToRoleAsync(
                            existingUser,
                            role);


                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(
                            ", ",
                            roleResult.Errors.Select(e => e.Description));

                        throw new Exception(
                            $"Failed to assign role '{role}' to existing user '{email}': {errors}");
                    }
                }
            }
        }
    }
}