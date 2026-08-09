using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using FleetCarePro.Models;
using FleetCarePro.Validation;

namespace FleetCarePro.ViewModels
{
    public class VehicleViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(17, MinimumLength = 17)]
        [ValidVIN]
        public string VIN { get; set; } = string.Empty;

        [Required]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        public string Make { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal PurchasePrice { get; set; }

        public VehicleStatus Status { get; set; }

        public int Mileage { get; set; }

        public IFormFile? VehicleImage { get; set; }

        public string? ExistingImageUrl { get; set; }

        public string? DriverId { get; set; }

        public List<ApplicationUser> Drivers { get; set; }
            = new List<ApplicationUser>();
    }
}