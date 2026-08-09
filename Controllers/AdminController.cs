using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FleetCarePro.Models;
using FleetCarePro.ViewModels;
using FleetCarePro.Filters;

namespace FleetCarePro.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = userManager.Users.ToList();

            var result = new List<ManageUserVM>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                result.Add(new ManageUserVM
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    EmployeeId = user.EmployeeId,
                    CurrentRole =
                        roles.FirstOrDefault() ?? "No Role"
                });
            }

            ViewBag.AllRoles =
                roleManager.Roles
                    .Select(r => r.Name)
                    .ToList();

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuditLog]
        public async Task<IActionResult> ChangeRole(
            string userId,
            string newRole)
        {
            var user =
                await userManager.FindByIdAsync(userId);

            if (user == null ||
                string.IsNullOrEmpty(newRole))
            {
                return RedirectToAction("ManageUsers");
            }

            // Make sure the requested role actually exists in
            // the system (Admin / FleetManager / Driver) instead
            // of trusting whatever string was posted.
            bool roleExists =
                await roleManager.RoleExistsAsync(newRole);

            if (!roleExists)
            {
                TempData["Error"] =
                    "Selected role does not exist.";

                return RedirectToAction("ManageUsers");
            }

            // Safety check: an Admin must not be able to remove
            // their own Admin role. Without this, an Admin could
            // accidentally lock themselves out of the Admin area
            // with no easy way back in except editing the database
            // directly.
            string? currentUserId =
                userManager.GetUserId(User);

            bool isActingOnSelf =
                string.Equals(
                    user.Id,
                    currentUserId,
                    StringComparison.OrdinalIgnoreCase);

            if (isActingOnSelf &&
                !string.Equals(
                    newRole,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] =
                    "You cannot remove your own Admin role.";

                return RedirectToAction("ManageUsers");
            }

            var currentRoles =
                await userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);
            }

            await userManager.AddToRoleAsync(
                user,
                newRole);

            TempData["Message"] =
                $"{user.Email}'s role updated to {newRole}.";

            return RedirectToAction("ManageUsers");
        }
    }
}