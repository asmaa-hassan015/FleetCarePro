namespace FleetCarePro.Models
{
    public class VendorService
    {
        public int ServiceCenterId { get; set; }
        public ServiceCenter ServiceCenter { get; set; } = null!;

        public int ServiceCategoryId { get; set; }
        public ServiceCategory ServiceCategory { get; set; } = null!;
    }
}