using Microsoft.AspNetCore.Mvc;
using FleetCarePro.Data;
using FleetCarePro.ViewModels;

namespace FleetCarePro.ViewComponents
{
    public class FleetCostSummaryViewComponent : ViewComponent
    {
        FleetContext db;

        public FleetCostSummaryViewComponent(FleetContext context)
        {
            db = context;
        }

        public IViewComponentResult Invoke()
        {
            var now = DateTime.Now;

            var query = db.ServiceRecords
                .Where(sr => sr.ServiceDate.Year == now.Year && sr.ServiceDate.Month == now.Month);

            var vm = new FleetCostSummaryVM
            {
                TotalCostThisMonth = query.Sum(sr => (decimal?)sr.TotalCost) ?? 0,
                RecordsCountThisMonth = query.Count(),
                MonthName = now.ToString("MMMM yyyy")
            };

            return View(vm);
        }
    }
}