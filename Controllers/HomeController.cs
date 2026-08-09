using FleetCarePro.Data;
using FleetCarePro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FleetCarePro.Controllers
{
    public class HomeController : Controller
    {
        private readonly FleetContext db;

        public HomeController(FleetContext context)
        {
            db = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            ViewBag.VehiclesCount = db.Vehicles.Count();

            ViewBag.ServiceCentersCount =
                db.ServiceCenters.Count();

            ViewBag.ServiceCategoriesCount =
                db.ServiceCategories.Count();

            int totalRecords =
                db.ServiceRecords.Count();

            int onTimeRecords =
                db.ServiceRecords.Count(sr =>
                    sr.Status == ServiceRecordStatus.Completed ||
                    sr.Status == ServiceRecordStatus.Approved);

            ViewBag.OnTimePercentage =
                totalRecords == 0
                    ? 100
                    : (int)(
                        (onTimeRecords /
                        (double)totalRecords) * 100);

            return View();
        }

        public IActionResult StatusCodeError(int id)
        {
            ViewBag.StatusCode = id;

            if (id == 404)
            {
                return View("NotFound");
            }

            return View("ServerError");
        }

        public IActionResult Maintenance()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            });
        }
    }
}