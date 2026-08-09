namespace FleetCarePro.ViewModels
{
    public class FleetCostSummaryVM
    {
        public decimal TotalCostThisMonth { get; set; }
        public int RecordsCountThisMonth { get; set; }
        public string MonthName { get; set; } = string.Empty;
    }
}