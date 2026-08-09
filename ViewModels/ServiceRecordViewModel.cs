using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using FleetCarePro.Models;

namespace FleetCarePro.ViewModels
{
    public class ServiceRecordViewModel
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int ServiceCenterId { get; set; }

        [Required]
        public DateTime ServiceDate { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int CurrentMileage { get; set; }

        public string? Notes { get; set; }

        // Invoice Upload
        // Allowed: .pdf, .jpg, .png
        // Max size: 5 MB
        public IFormFile? InvoiceDocument { get; set; }

        // Master-Detail
        [MinLength(1, ErrorMessage = "At least one service line item is required.")]
        public List<ServiceLineItemViewModel> ServiceLineItems { get; set; }
            = new List<ServiceLineItemViewModel>();

        // Dropdown data sources
        public List<Vehicle> Vehicles { get; set; }
            = new List<Vehicle>();

        public List<ServiceCenter> ServiceCenters { get; set; }
            = new List<ServiceCenter>();

        public List<ServiceCategory> ServiceCategories { get; set; }
            = new List<ServiceCategory>();
    }
}