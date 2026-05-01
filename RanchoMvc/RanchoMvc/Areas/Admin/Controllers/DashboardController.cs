using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using RanchoMvc.Data;
using RanchoMvc.Filters;
using RanchoMvc.Models;

namespace RanchoMvc.Areas.Admin.Controllers
{
    [ModuleAuthorize]
    public class DashboardController : BaseAdminController
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        public ActionResult Index(string permError = null)
        {
            if (!string.IsNullOrEmpty(permError))
                TempData["Error"] = "No tienes permiso para acceder a esa sección.";

            // KPI cards
            ViewBag.TotalPlans          = _db.Plans.Count(p => p.IsActive);
            ViewBag.PendingReservations = _db.Reservations.Count(r => r.Status == "Pendiente");
            ViewBag.UnreadMessages      = _db.ContactMessages.Count(m => !m.IsRead);
            ViewBag.TotalGallery        = _db.GalleryImages.Count(g => g.IsActive);

            // Ingresos estimados (plan.Price × guestCount de reservaciones confirmadas)
            var confirmedRes = _db.Reservations
                .Include("Plan")
                .Where(r => r.Status == "Confirmada" && r.Plan != null)
                .ToList();
            ViewBag.EstimatedRevenue = confirmedRes.Sum(r => r.Plan.Price * r.GuestCount);
            ViewBag.ConfirmedCount   = confirmedRes.Count;

            // Actividad reciente
            ViewBag.RecentReservations = _db.Reservations
                .Include("Plan")
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();
            ViewBag.RecentMessages = _db.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToList();

            // Chart: reservaciones por mes (últimos 6 meses)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
            var cutoff = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);
            var byMonth = _db.Reservations
                .Where(r => r.CreatedAt >= cutoff)
                .ToList()
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            var monthLabels = new List<string>();
            var monthCounts = new List<int>();
            string[] meses = { "", "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            for (int i = 5; i >= 0; i--)
            {
                var d = DateTime.UtcNow.AddMonths(-i);
                var item = byMonth.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
                monthLabels.Add(meses[d.Month] + " " + d.Year.ToString().Substring(2));
                monthCounts.Add(item != null ? item.Count : 0);
            }
            ViewBag.ChartMonthLabels = JsonConvert.SerializeObject(monthLabels);
            ViewBag.ChartMonthData   = JsonConvert.SerializeObject(monthCounts);

            // Chart: por estado
            var byStatus = _db.Reservations
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();
            var statusLabels = byStatus.Select(x => x.Status).ToList();
            var statusCounts = byStatus.Select(x => x.Count).ToList();
            ViewBag.ChartStatusLabels = JsonConvert.SerializeObject(statusLabels);
            ViewBag.ChartStatusData   = JsonConvert.SerializeObject(statusCounts);

            // Chart: por plan
            var byPlan = _db.Reservations
                .Include("Plan")
                .ToList()
                .GroupBy(r => r.Plan != null ? r.Plan.Title : "Sin plan")
                .Select(g => new { Plan = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();
            ViewBag.ChartPlanLabels = JsonConvert.SerializeObject(byPlan.Select(x => x.Plan).ToList());
            ViewBag.ChartPlanData   = JsonConvert.SerializeObject(byPlan.Select(x => x.Count).ToList());

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
