using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetCarePro.Data;
using FleetCarePro.Models;
using FleetCarePro.ViewModels;
using FleetCarePro.Filters;

namespace FleetCarePro.Controllers
{
    public class ServiceCenterController : Controller
    {
        private readonly FleetContext db;
        private readonly IMapper mapper;

        public ServiceCenterController(
            FleetContext context,
            IMapper mapper)
        {
            db = context;
            this.mapper = mapper;
        }

        [Authorize]
        public IActionResult GetAll()
        {
            var centers = db.ServiceCenters
                .Include(c => c.VendorServices)
                    .ThenInclude(vs => vs.ServiceCategory)
                .ToList();

            return View("GetAll", centers);
        }

        [Authorize]
        public IActionResult Details(int id)
        {
            var center = db.ServiceCenters
                .Include(c => c.ServiceRecords)
                .Include(c => c.VendorServices)
                    .ThenInclude(vs => vs.ServiceCategory)
                .FirstOrDefault(c => c.Id == id);

            if (center == null)
            {
                return RedirectToAction("GetAll");
            }

            return View(center);
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpGet]
        public IActionResult Add()
        {
            var vm = new ServiceCenterViewModel
            {
                ServiceCategories = db.ServiceCategories.ToList()
            };

            return View(vm);
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult addNew(ServiceCenterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.ServiceCategories =
                    db.ServiceCategories.ToList();

                return View("Add", vm);
            }

            using var transaction =
                db.Database.BeginTransaction();

            try
            {
                var center =
                    mapper.Map<ServiceCenter>(vm);

                db.ServiceCenters.Add(center);

                // Save first so the generated ServiceCenter Id
                // can be used by VendorService.
                db.SaveChanges();

                var selectedIds =
                    vm.SelectedServiceCategoryIds?
                        .Distinct()
                        .ToList()
                    ?? new List<int>();

                foreach (var categoryId in selectedIds)
                {
                    // Make sure the selected category actually exists.
                    bool categoryExists =
                        db.ServiceCategories
                            .Any(c => c.Id == categoryId);

                    if (!categoryExists)
                    {
                        ModelState.AddModelError(
                            nameof(vm.SelectedServiceCategoryIds),
                            "One or more selected service categories are invalid.");

                        transaction.Rollback();

                        vm.ServiceCategories =
                            db.ServiceCategories.ToList();

                        return View("Add", vm);
                    }

                    db.VendorServices.Add(
                        new VendorService
                        {
                            ServiceCenterId = center.Id,
                            ServiceCategoryId = categoryId
                        });
                }

                db.SaveChanges();

                transaction.Commit();

                TempData["Message"] =
                    "Service Center added successfully.";

                HttpContext.Session.SetString(
                    "LastAction",
                    $"Added Service Center {center.Name} at {DateTime.Now:g}");

                return RedirectToAction("GetAll");
            }
            catch
            {
                transaction.Rollback();

                ModelState.AddModelError(
                    "",
                    "An error occurred while saving the service center.");

                vm.ServiceCategories =
                    db.ServiceCategories.ToList();

                return View("Add", vm);
            }
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpGet]
        public IActionResult Update(int id)
        {
            var center = db.ServiceCenters
                .Include(c => c.VendorServices)
                .FirstOrDefault(c => c.Id == id);

            if (center == null)
            {
                return RedirectToAction("GetAll");
            }

            var vm =
                mapper.Map<ServiceCenterViewModel>(center);

            vm.Id = center.Id;

            vm.ServiceCategories =
                db.ServiceCategories.ToList();

            vm.SelectedServiceCategoryIds =
                center.VendorServices
                    .Select(vs => vs.ServiceCategoryId)
                    .ToList();

            return View(vm);
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(
            ServiceCenterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.ServiceCategories =
                    db.ServiceCategories.ToList();

                return View("Update", vm);
            }

            var center = db.ServiceCenters
                .Include(c => c.VendorServices)
                .FirstOrDefault(c => c.Id == vm.Id);

            if (center == null)
            {
                vm.ServiceCategories =
                    db.ServiceCategories.ToList();

                return View("Update", vm);
            }

            var selectedIds =
                vm.SelectedServiceCategoryIds?
                    .Distinct()
                    .ToList()
                ?? new List<int>();

            // Validate selected categories before modifying
            // the existing relationships.
            bool allCategoriesExist =
                selectedIds.All(categoryId =>
                    db.ServiceCategories
                        .Any(c => c.Id == categoryId));

            if (!allCategoriesExist)
            {
                ModelState.AddModelError(
                    nameof(vm.SelectedServiceCategoryIds),
                    "One or more selected service categories are invalid.");

                vm.ServiceCategories =
                    db.ServiceCategories.ToList();

                return View("Update", vm);
            }

            using var transaction =
                db.Database.BeginTransaction();

            try
            {
                mapper.Map(vm, center);

                // Replace existing N-to-N relationships.
                db.VendorServices.RemoveRange(
                    center.VendorServices);

                foreach (var categoryId in selectedIds)
                {
                    db.VendorServices.Add(
                        new VendorService
                        {
                            ServiceCenterId = center.Id,
                            ServiceCategoryId = categoryId
                        });
                }

                db.SaveChanges();

                transaction.Commit();

                TempData["Message"] =
                    "Service Center updated successfully.";

                HttpContext.Session.SetString(
                    "LastAction",
                    $"Updated Service Center #{center.Id} at {DateTime.Now:g}");

                return RedirectToAction("GetAll");
            }
            catch
            {
                transaction.Rollback();

                ModelState.AddModelError(
                    "",
                    "An error occurred while updating the service center.");

                vm.ServiceCategories =
                    db.ServiceCategories.ToList();

                return View("Update", vm);
            }
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var center = db.ServiceCenters
                .Include(c => c.ServiceRecords)
                .Include(c => c.VendorServices)
                .FirstOrDefault(c => c.Id == id);

            if (center == null)
            {
                return RedirectToAction("GetAll");
            }

            // ServiceRecord -> ServiceCenter uses Restrict delete behavior.
            // Therefore, do not delete a ServiceCenter that has service history.
            if (center.ServiceRecords.Any())
            {
                TempData["Error"] =
                    "This Service Center cannot be deleted because it has service records.";

                return RedirectToAction("GetAll");
            }

            using var transaction =
                db.Database.BeginTransaction();

            try
            {
                // Remove N-to-N relationships first.
                db.VendorServices.RemoveRange(
                    center.VendorServices);

                db.ServiceCenters.Remove(center);

                db.SaveChanges();

                transaction.Commit();

                TempData["Message"] =
                    "Service Center deleted successfully.";

                HttpContext.Session.SetString(
                    "LastAction",
                    $"Deleted Service Center #{id} at {DateTime.Now:g}");
            }
            catch
            {
                transaction.Rollback();

                TempData["Error"] =
                    "An error occurred while deleting the service center.";
            }

            return RedirectToAction("GetAll");
        }
    }
}