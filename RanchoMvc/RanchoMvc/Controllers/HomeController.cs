using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using RanchoMvc.Data;
using RanchoMvc.Models.ViewModels;

namespace RanchoMvc.Controllers
{
    public class HomeController : BaseController
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        public ActionResult Index()
        {
            var vm = new HomeViewModel
            {
                Plans = _db.Plans.Where(p => p.IsActive).OrderBy(p => p.Order).ToList(),
                GalleryImages = _db.GalleryImages.Where(g => g.IsActive).OrderBy(g => g.Order).Take(18).ToList(),
                Settings = _db.SiteSettings.ToDictionary(s => s.Key, s => s.Value),
                ReservationForm = new ReservationViewModel
                {
                    Plans = _db.Plans
                        .Where(p => p.IsActive)
                        .OrderBy(p => p.Order)
                        .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Title })
                        .ToList()
                },
                ContactForm = new ContactViewModel()
            };

            var mpToken = ConfigurationManager.AppSettings["MercadoPagoAccessToken"] ?? "";
            ViewBag.OnlinePaymentEnabled = !string.IsNullOrEmpty(mpToken) && mpToken != "REEMPLAZAR";
            ViewBag.FooterPlans = vm.Plans;

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
