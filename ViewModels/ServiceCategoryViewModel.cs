using System.ComponentModel.DataAnnotations;

namespace FleetCarePro.ViewModels
{
    public class ServiceCategoryViewModel
    {
        [Required]
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int RecommendedIntervalMonths { get; set; }
    }
}