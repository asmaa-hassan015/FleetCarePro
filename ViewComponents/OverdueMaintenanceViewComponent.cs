using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetCarePro.Data;
using FleetCarePro.Models;

namespace FleetCarePro.ViewComponents
{
    public class OverdueMaintenanceViewComponent : ViewComponent
    {
        FleetContext db;

        public OverdueMaintenanceViewComponent(FleetContext context)
        {
            db = context;
        }

        public IViewComponentResult Invoke()
        {
            DateTime sixMonthsAgo = DateTime.Now.AddMonths(-6);

            // A vehicle is "overdue" if it has never been serviced,
            // or its most recent service record is older than 6 months.
            var vehicles = db.Vehicles
                .Include(v => v.ServiceRecords)
                .Where(v => v.Status != VehicleStatus.Decommissioned)
                .Where(v => !v.ServiceRecords.Any() ||
                            v.ServiceRecords.Max(sr => sr.ServiceDate) < sixMonthsAgo)
                .ToList();

            return View(vehicles);
        }
    }
}