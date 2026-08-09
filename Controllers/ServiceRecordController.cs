using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetCarePro.Data;
using FleetCarePro.Models;
using FleetCarePro.ViewModels;
using FleetCarePro.Filters;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace FleetCarePro.Controllers
{
    public class ServiceRecordController : Controller
    {
        private readonly FleetContext db;
        private readonly IWebHostEnvironment env;
        private readonly IMapper mapper;

        private static readonly string[] AllowedInvoiceExtensions =
        {
            ".pdf",
            ".jpg",
            ".png"
        };

        private const long MaxInvoiceFileSize = 5 * 1024 * 1024;

        public ServiceRecordController(
            FleetContext context,
            IWebHostEnvironment webHostEnvironment,
            IMapper mapper)
        {
            db = context;
            env = webHostEnvironment;
            this.mapper = mapper;
        }

        [Authorize]
        public IActionResult GetAll()
        {
            var records = db.ServiceRecords
                .Include(sr => sr.Vehicle)
                .Include(sr => sr.ServiceCenter)
                .ToList();

            return View("GetAll", records);
        }

        [Authorize]
        public IActionResult Details(int id)
        {
            var record = db.ServiceRecords
                .Include(sr => sr.Vehicle)
                .Include(sr => sr.ServiceCenter)
                .Include(sr => sr.ServiceLineItems)
                .ThenInclude(s => s.ServiceCategory)
                .FirstOrDefault(sr => sr.Id == id);

            if (record == null)
            {
                return RedirectToAction("GetAll");
            }

            return View(record);
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpGet]
        public IActionResult Add()
        {
            var vm = new ServiceRecordViewModel
            {
                Vehicles = db.Vehicles.ToList(),
                ServiceCenters = db.ServiceCenters.ToList(),
                ServiceCategories = db.ServiceCategories.ToList(),
                ServiceDate = DateTime.Now
            };

            return View(vm);
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(ServiceRecordViewModel vm)
        {
            // Validate invoice before starting the transaction or saving files.
            if (vm.InvoiceDocument != null)
            {
                ValidateInvoice(vm.InvoiceDocument);
            }

            if (!ModelState.IsValid)
            {
                LoadDropdownData(vm);
                return View(vm);
            }

            string? newInvoicePath = null;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var record = new ServiceRecord
                    {
                        VehicleId = vm.VehicleId,
                        ServiceCenterId = vm.ServiceCenterId,
                        ServiceDate = vm.ServiceDate,
                        CurrentMileage = vm.CurrentMileage,
                        Notes = vm.Notes,
                        Status = ServiceRecordStatus.Pending,
                        CreatedByUserId =
                            User.FindFirstValue(ClaimTypes.NameIdentifier),

                        // Master-Detail total
                        TotalCost = vm.ServiceLineItems.Sum(x => x.Cost)
                    };

                    // Invoice Upload
                    if (vm.InvoiceDocument != null)
                    {
                        newInvoicePath = SaveInvoice(vm.InvoiceDocument);
                        record.InvoiceDocumentPath = newInvoicePath;
                    }

                    // Save Master
                    db.ServiceRecords.Add(record);
                    db.SaveChanges();

                    // Save Details
                    foreach (var item in vm.ServiceLineItems)
                    {
                        var line = mapper.Map<ServiceLineItem>(item);

                        line.ServiceRecordId = record.Id;

                        db.ServiceLineItems.Add(line);
                    }

                    db.SaveChanges();

                    transaction.Commit();

                    TempData["Message"] =
                        "Service Record added successfully.";

                    HttpContext.Session.SetString(
                        "LastAction",
                        $"Added Service Record #{record.Id} at {DateTime.Now:g}");

                    return RedirectToAction("GetAll");
                }
                catch
                {
                    transaction.Rollback();

                    // Remove uploaded file if database operation failed.
                    DeleteInvoiceFile(newInvoicePath);

                    ModelState.AddModelError(
                        "",
                        "An error occurred while saving the service record.");

                    LoadDropdownData(vm);

                    return View(vm);
                }
            }
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [HttpGet]
        public IActionResult Update(int id)
        {
            var record = db.ServiceRecords
                .Include(sr => sr.ServiceLineItems)
                .FirstOrDefault(sr => sr.Id == id);

            if (record == null)
            {
                return RedirectToAction("GetAll");
            }

            ViewBag.RecordId = id;

            var vm = new ServiceRecordViewModel
            {
                VehicleId = record.VehicleId,
                ServiceCenterId = record.ServiceCenterId,
                ServiceDate = record.ServiceDate,
                CurrentMileage = record.CurrentMileage,
                Notes = record.Notes,

                Vehicles = db.Vehicles.ToList(),
                ServiceCenters = db.ServiceCenters.ToList(),
                ServiceCategories = db.ServiceCategories.ToList()
            };

            foreach (var item in record.ServiceLineItems)
            {
                vm.ServiceLineItems.Add(
                    new ServiceLineItemViewModel
                    {
                        ServiceCategoryId = item.ServiceCategoryId,
                        Description = item.Description,
                        Cost = item.Cost
                    });
            }

            return View(vm);
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(
            int id,
            ServiceRecordViewModel vm)
        {
            var record = db.ServiceRecords
                .Include(sr => sr.ServiceLineItems)
                .FirstOrDefault(sr => sr.Id == id);

            if (record == null)
            {
                return RedirectToAction("GetAll");
            }

            // Validate new invoice only if one was uploaded.
            if (vm.InvoiceDocument != null)
            {
                ValidateInvoice(vm.InvoiceDocument);
            }

            if (!ModelState.IsValid)
            {
                LoadDropdownData(vm);
                ViewBag.RecordId = id;

                return View(vm);
            }

            string? newInvoicePath = null;
            string? oldInvoicePath = record.InvoiceDocumentPath;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Update Master
                    record.VehicleId = vm.VehicleId;
                    record.ServiceCenterId = vm.ServiceCenterId;
                    record.ServiceDate = vm.ServiceDate;
                    record.CurrentMileage = vm.CurrentMileage;
                    record.Notes = vm.Notes;

                    // Recalculate TotalCost from Detail items.
                    record.TotalCost =
                        vm.ServiceLineItems.Sum(x => x.Cost);

                    // Update Invoice only when a new file is supplied.
                    if (vm.InvoiceDocument != null)
                    {
                        newInvoicePath =
                            SaveInvoice(vm.InvoiceDocument);

                        record.InvoiceDocumentPath =
                            newInvoicePath;
                    }

                    // Remove old Detail records.
                    db.ServiceLineItems.RemoveRange(
                        record.ServiceLineItems);

                    // Add updated Detail records.
                    foreach (var item in vm.ServiceLineItems)
                    {
                        var line =
                            mapper.Map<ServiceLineItem>(item);

                        line.ServiceRecordId = record.Id;

                        db.ServiceLineItems.Add(line);
                    }

                    db.SaveChanges();

                    transaction.Commit();

                    // Delete old invoice only after successful DB update.
                    if (newInvoicePath != null &&
                        !string.IsNullOrEmpty(oldInvoicePath))
                    {
                        DeleteInvoiceFile(oldInvoicePath);
                    }

                    TempData["Message"] =
                        "Service Record updated successfully.";

                    HttpContext.Session.SetString(
                        "LastAction",
                        $"Updated Service Record #{record.Id} at {DateTime.Now:g}");

                    return RedirectToAction("GetAll");
                }
                catch
                {
                    transaction.Rollback();

                    // Delete newly uploaded file if update failed.
                    DeleteInvoiceFile(newInvoicePath);

                    ModelState.AddModelError(
                        "",
                        "An error occurred while updating the service record.");

                    LoadDropdownData(vm);
                    ViewBag.RecordId = id;

                    return View(vm);
                }
            }
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var record = db.ServiceRecords.Find(id);

            if (record != null)
            {
                record.Status = ServiceRecordStatus.Approved;

                db.SaveChanges();

                TempData["Message"] =
                    $"Service Record #{id} approved.";

                HttpContext.Session.SetString(
                    "LastAction",
                    $"Approved Service Record #{id} at {DateTime.Now:g}");
            }

            return RedirectToAction("GetAll");
        }

        [Authorize(Policy = "ManagerOrAdmin")]
        [AuditLog]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var record = db.ServiceRecords
                .Include(sr => sr.ServiceLineItems)
                .FirstOrDefault(sr => sr.Id == id);

            if (record != null)
            {
                string? invoicePath =
                    record.InvoiceDocumentPath;

                db.ServiceLineItems.RemoveRange(
                    record.ServiceLineItems);

                db.ServiceRecords.Remove(record);

                db.SaveChanges();

                // Remove associated invoice after successful deletion.
                DeleteInvoiceFile(invoicePath);

                TempData["Message"] =
                    "Service Record deleted successfully.";

                HttpContext.Session.SetString(
                    "LastAction",
                    $"Deleted Service Record #{id} at {DateTime.Now:g}");
            }

            return RedirectToAction("GetAll");
        }

        // ---------------------------------------------------------
        // Helper Methods
        // ---------------------------------------------------------

        private void LoadDropdownData(ServiceRecordViewModel vm)
        {
            vm.Vehicles = db.Vehicles.ToList();
            vm.ServiceCenters = db.ServiceCenters.ToList();
            vm.ServiceCategories = db.ServiceCategories.ToList();
        }

        private void ValidateInvoice(IFormFile file)
        {
            string extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (!AllowedInvoiceExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(ServiceRecordViewModel.InvoiceDocument),
                    "Only PDF, JPG and PNG files are allowed.");

                return;
            }

            if (file.Length > MaxInvoiceFileSize)
            {
                ModelState.AddModelError(
                    nameof(ServiceRecordViewModel.InvoiceDocument),
                    "Maximum file size is 5 MB.");
            }
        }

        private string SaveInvoice(IFormFile file)
        {
            string uploadsFolder = Path.Combine(
                env.WebRootPath,
                "uploads",
                "invoices");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            string fileName =
                Guid.NewGuid().ToString() + extension;

            string filePath =
                Path.Combine(uploadsFolder, fileName);

            using (var stream =
                   new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/uploads/invoices/" + fileName;
        }

        private void DeleteInvoiceFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            string fileName =
                Path.GetFileName(relativePath);

            string filePath = Path.Combine(
                env.WebRootPath,
                "uploads",
                "invoices",
                fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}