using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetCarePro.Data;
using FleetCarePro.Models;
using FleetCarePro.ViewModels;
using FleetCarePro.Filters;
using FleetCarePro.Helpers;
using System.Security.Claims;

namespace FleetCarePro.Controllers
{
    public class VehicleController : Controller
    {
        private readonly FleetContext db;
        private readonly IWebHostEnvironment env;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;

        public VehicleController(
            FleetContext context,
            IWebHostEnvironment environment,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            db = context;
            env = environment;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [Authorize]
        public IActionResult GetAll(int pageIndex = 1)
        {
            int pageSize = 5;

            var query = db.Vehicles
                .Include(v => v.Driver)
                .OrderBy(v => v.Id)
                .AsQueryable();

            if (User.IsInRole("Driver"))
            {
                string? currentUserId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                query = query.Where(v => v.DriverId == currentUserId);
            }

            var vehicles = PaginatedList<Vehicle>.Create(
                query,
                pageIndex,
                pageSize);

            var statusSummary = db.Vehicles
                .GroupBy(v => v.Status)
                .Select(g => new
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToDictionary(
                    x => x.Status,
                    x => x.Count);

            ViewBag.StatusSummary = statusSummary;

            return View("GetAll", vehicles);
        }

        [Authorize(Policy = "DriverOnly")]
        public IActionResult MyVehicles()
        {
            string? currentUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            var vehicles = db.Vehicles
                .Include(v => v.Driver)
                .Where(v => v.DriverId == currentUserId)
                .ToList();

            ViewData["Title"] = "My Vehicles";

            return View("Cards", vehicles);
        }

        [Authorize]
        public IActionResult Cards()
        {
            var query = db.Vehicles
                .Include(v => v.Driver)
                .AsQueryable();

            if (User.IsInRole("Driver"))
            {
                string? currentUserId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                query = query.Where(
                    v => v.DriverId == currentUserId);
            }

            var vehicles = query.ToList();

            return View(vehicles);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Filter(
            string? status,
            string? make)
        {
            var query = db.Vehicles
                .Include(v => v.Driver)
                .AsQueryable();

            if (User.IsInRole("Driver"))
            {
                string? currentUserId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                query = query.Where(
                    v => v.DriverId == currentUserId);
            }

            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<VehicleStatus>(
                    status,
                    out var parsedStatus))
            {
                query = query.Where(
                    v => v.Status == parsedStatus);
            }

            if (!string.IsNullOrEmpty(make))
            {
                query = query.Where(
                    v => v.Make.Contains(make));
            }

            var vehicles = query.ToList();

            return PartialView(
                "_VehicleTableRows",
                vehicles);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await db.Vehicles
                .Include(v => v.Driver)
                .Include(v => v.ServiceRecords)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return RedirectToAction("GetAll");
            }

            if (User.IsInRole("Driver"))
            {
                string? currentUserId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (vehicle.DriverId != currentUserId)
                {
                    return Forbid();
                }
            }

            return View(vehicle);
        }

        // =========================
        // ADD
        // =========================

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var drivers =
                await userManager.GetUsersInRoleAsync("Driver");

            var vm = new VehicleViewModel
            {
                Drivers = drivers.ToList()
            };

            return View(vm);
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNew(
            VehicleViewModel vm)
        {
            ValidateVehicleImage(vm);

            if (!ModelState.IsValid)
            {
                var drivers =
                    await userManager.GetUsersInRoleAsync("Driver");

                vm.Drivers = drivers.ToList();

                return View("Add", vm);
            }

            var vehicle = mapper.Map<Vehicle>(vm);

            if (vm.VehicleImage != null &&
                vm.VehicleImage.Length > 0)
            {
                vehicle.VehicleImageURL =
                    await SaveVehicleImageAsync(
                        vm.VehicleImage);
            }

            db.Vehicles.Add(vehicle);

            await db.SaveChangesAsync();

            TempData["Message"] =
                "Vehicle added successfully.";

            HttpContext.Session.SetString(
                "LastAction",
                $"Added Vehicle {vehicle.Make} {vehicle.Model} at {DateTime.Now:g}");

            return RedirectToAction("GetAll");
        }

        // =========================
        // UPDATE
        // =========================

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var vehicle =
                await db.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return RedirectToAction("GetAll");
            }

            var vm =
                mapper.Map<VehicleViewModel>(vehicle);

            vm.Id = vehicle.Id;

            vm.ExistingImageUrl =
                vehicle.VehicleImageURL;

            var drivers =
                await userManager.GetUsersInRoleAsync("Driver");

            vm.Drivers = drivers.ToList();

            return View(vm);
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            VehicleViewModel vm)
        {
            ValidateVehicleImage(vm);

            if (!ModelState.IsValid)
            {
                var drivers =
                    await userManager.GetUsersInRoleAsync("Driver");

                vm.Drivers = drivers.ToList();

                return View("Update", vm);
            }

            var vehicle =
                await db.Vehicles.FindAsync(vm.Id);

            if (vehicle == null)
            {
                return RedirectToAction("GetAll");
            }

            mapper.Map(vm, vehicle);

            if (vm.VehicleImage != null &&
                vm.VehicleImage.Length > 0)
            {
                vehicle.VehicleImageURL =
                    await SaveVehicleImageAsync(
                        vm.VehicleImage);
            }

            await db.SaveChangesAsync();

            TempData["Message"] =
                "Vehicle updated successfully.";

            HttpContext.Session.SetString(
                "LastAction",
                $"Updated Vehicle #{vehicle.Id} at {DateTime.Now:g}");

            return RedirectToAction("GetAll");
        }

        // =========================
        // DECOMMISSION
        // =========================

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicle =
                await db.Vehicles.FindAsync(id);

            if (vehicle != null)
            {
                vehicle.Status =
                    VehicleStatus.Decommissioned;

                await db.SaveChangesAsync();

                TempData["Message"] =
                    "Vehicle decommissioned successfully.";

                HttpContext.Session.SetString(
                    "LastAction",
                    $"Decommissioned Vehicle #{id} at {DateTime.Now:g}");
            }

            return RedirectToAction("GetAll");
        }

        // =========================
        // IMAGE VALIDATION
        // =========================

        private void ValidateVehicleImage(
            VehicleViewModel vm)
        {
            if (vm.VehicleImage == null ||
                vm.VehicleImage.Length == 0)
            {
                return;
            }

            const long maxFileSize =
                5 * 1024 * 1024;

            if (vm.VehicleImage.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    "VehicleImage",
                    "Image size must not exceed 5 MB.");

                return;
            }

            var allowedExtensions =
                new[] { ".jpg", ".jpeg", ".png" };

            var extension =
                Path.GetExtension(
                    vm.VehicleImage.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "VehicleImage",
                    "Only JPG, JPEG and PNG images are allowed.");

                return;
            }

            var allowedContentTypes =
                new[]
                {
                    "image/jpeg",
                    "image/png"
                };

            if (!allowedContentTypes.Contains(
                    vm.VehicleImage.ContentType
                        .ToLowerInvariant()))
            {
                ModelState.AddModelError(
                    "VehicleImage",
                    "Invalid image file type.");
            }
        }

        // =========================
        // SAVE IMAGE
        // =========================

        private async Task<string> SaveVehicleImageAsync(
            IFormFile file)
        {
            string uploadsFolder =
                Path.Combine(
                    env.WebRootPath,
                    "uploads",
                    "vehicles");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(
                    uploadsFolder);
            }

            string extension =
                Path.GetExtension(
                    file.FileName)
                    .ToLowerInvariant();

            string uniqueFileName =
                $"{Guid.NewGuid()}{extension}";

            string filePath =
                Path.Combine(
                    uploadsFolder,
                    uniqueFileName);

            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);

            await file.CopyToAsync(stream);

            return
                $"/uploads/vehicles/{uniqueFileName}";
        }
    }
}