using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using RanchoMvc.Data;
using RanchoMvc.Filters;
using RanchoMvc.Models;

namespace RanchoMvc.Areas.Admin.Controllers
{
    [ModuleAuthorize(Module = AdminModule.Settings)]
    public class SettingsController : BaseAdminController
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        private static readonly List<string> GroupOrder = new List<string>
            { "General", "Apariencia", "Pagos", "Hero", "About", "CTA", "Contacto", "Redes" };

        public ActionResult Index()
        {
            var settings = _db.SiteSettings.OrderBy(s => s.Id).ToList();
            var existing = settings.Select(s => s.Group).Distinct().ToList();
            ViewBag.Groups = GroupOrder.Where(g => existing.Contains(g))
                .Concat(existing.Where(g => !GroupOrder.Contains(g)))
                .ToList();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(FormCollection form)
        {
            var settings = _db.SiteSettings.ToList();
            foreach (var setting in settings)
            {
                var val = form["setting_" + setting.Id];
                if (val != null)
                    setting.Value = val;
            }
            _db.SaveChanges();
            System.Web.HttpRuntime.Cache.Remove("SiteLogoUrl");
            System.Web.HttpRuntime.Cache.Remove("SiteSettings");
            TempData["Success"] = "Ajustes guardados correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return Json(new { success = false, message = "No se seleccionó ningún archivo." });

            var cloudName = System.Configuration.ConfigurationManager.AppSettings["CloudinaryCloudName"];
            var apiKey    = System.Configuration.ConfigurationManager.AppSettings["CloudinaryApiKey"];
            var apiSecret = System.Configuration.ConfigurationManager.AppSettings["CloudinaryApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || cloudName == "REEMPLAZAR")
                return Json(new { success = false, message = "Cloudinary no está configurado en Web.config." });

            try
            {
                var cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
                var isVideo = file.ContentType.StartsWith("video/");

                string url;
                if (isVideo)
                {
                    var vp = new VideoUploadParams
                    {
                        File = new FileDescription(file.FileName, file.InputStream),
                        Folder = "rancho-experience"
                    };
                    var vr = cloudinary.Upload(vp);
                    if (vr.Error != null)
                        return Json(new { success = false, message = vr.Error.Message });
                    url = vr.SecureUrl.ToString();
                }
                else
                {
                    var ip = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, file.InputStream),
                        Folder = "rancho-experience",
                        Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                    };
                    var ir = cloudinary.Upload(ip);
                    if (ir.Error != null)
                        return Json(new { success = false, message = ir.Error.Message });
                    url = ir.SecureUrl.ToString();
                }

                return Json(new { success = true, url = url });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
