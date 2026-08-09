using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FleetCarePro.Validation;

namespace FleetCarePro.Models
{
    public enum VehicleStatus
    {
        Active,
        InService,
        Decommissioned
    }

    public class Vehicle
    {
        [Key]
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

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }

        public VehicleStatus Status { get; set; }

        public int Mileage { get; set; }

        public string? VehicleImageURL { get; set; }

        public string? DriverId { get; set; }
        public ApplicationUser? Driver { get; set; }

        public List<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
    }
}