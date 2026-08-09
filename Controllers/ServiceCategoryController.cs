using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetCarePro.Data;
using FleetCarePro.Models;
using FleetCarePro.Filters;

namespace FleetCarePro.Controllers
{
    public class ServiceCategoryController : Controller
    {
        private readonly FleetContext db;

        public ServiceCategoryController(FleetContext context)
        {
            db = context;
        }

        [Authorize]
        public IActionResult GetAll()
        {
            var categories = db.ServiceCategories
                .Include(c => c.VendorServices)
                    .ThenInclude(vs => vs.ServiceCenter)
                .ToList();

            return View("GetAll", categories);
        }

        [Authorize]
        public IActionResult Details(int id)
        {
            var category = db.ServiceCategories
                .Include(c => c.ServiceLineItems)
                    .ThenInclude(li => li.ServiceRecord)
                .Include(c => c.VendorServices)
                    .ThenInclude(vs => vs.ServiceCenter)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return RedirectToAction("GetAll");
            }

            return View(category);
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult addNew(ServiceCategory c)
        {
            if (!ModelState.IsValid)
            {
                return View("Add", c);
            }

            db.ServiceCategories.Add(c);
            db.SaveChanges();

            TempData["Message"] =
                "Service Category added successfully.";

            HttpContext.Session.SetString(
                "LastAction",
                $"Added Service Category {c.CategoryName} at {DateTime.Now:g}");

            return RedirectToAction("GetAll");
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpGet]
        public IActionResult Update(int id)
        {
            var category = db.ServiceCategories.Find(id);

            if (category == null)
            {
                return RedirectToAction("GetAll");
            }

            return View(category);
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(ServiceCategory c)
        {
            if (!ModelState.IsValid)
            {
                return View("Update", c);
            }

            var category = db.ServiceCategories.Find(c.Id);

            if (category == null)
            {
                return RedirectToAction("GetAll");
            }

            category.CategoryName =
                c.CategoryName;

            category.Description =
                c.Description;

            category.RecommendedIntervalMonths =
                c.RecommendedIntervalMonths;

            db.SaveChanges();

            TempData["Message"] =
                "Service Category updated successfully.";

            HttpContext.Session.SetString(
                "LastAction",
                $"Updated Service Category #{category.Id} at {DateTime.Now:g}");

            return RedirectToAction("GetAll");
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var category = db.ServiceCategories
                .Include(c => c.ServiceLineItems)
                .Include(c => c.VendorServices)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return RedirectToAction("GetAll");
            }

            // A category that is already used by ServiceLineItems
            // or VendorServices should not be deleted because
            // both relationships use Restrict delete behavior.
            if (category.ServiceLineItems.Any() ||
                category.VendorServices.Any())
            {
                TempData["Error"] =
                    "This Service Category cannot be deleted because it is already in use.";

                return RedirectToAction("GetAll");
            }

            db.ServiceCategories.Remove(category);
            db.SaveChanges();

            TempData["Message"] =
                "Service Category deleted successfully.";

            HttpContext.Session.SetString(
                "LastAction",
                $"Deleted Service Category #{id} at {DateTime.Now:g}");

            return RedirectToAction("GetAll");
        }
    }
}