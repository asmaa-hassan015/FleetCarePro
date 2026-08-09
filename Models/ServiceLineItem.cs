using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetCarePro.Models
{
    public class ServiceLineItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(ServiceRecord))]
        public int ServiceRecordId { get; set; }
        public ServiceRecord? ServiceRecord { get; set; }

        [ForeignKey(nameof(ServiceCategory))]
        public int ServiceCategoryId { get; set; }
        public ServiceCategory? ServiceCategory { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
    }
}