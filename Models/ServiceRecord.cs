using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetCarePro.Models
{
    public enum ServiceRecordStatus
    {
        Pending,
        Approved,
        Completed,
        Cancelled
    }

    public class ServiceRecord
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Vehicle))]
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        [ForeignKey(nameof(ServiceCenter))]
        public int ServiceCenterId { get; set; }
        public ServiceCenter? ServiceCenter { get; set; }

        public DateTime ServiceDate { get; set; }

        public int CurrentMileage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        public string? InvoiceDocumentPath { get; set; }

        public string? Notes { get; set; }

        public ServiceRecordStatus Status { get; set; }

        public string? CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        public List<ServiceLineItem> ServiceLineItems { get; set; } = new List<ServiceLineItem>();
    }
}