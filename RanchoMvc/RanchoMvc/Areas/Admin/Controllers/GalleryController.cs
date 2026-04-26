using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using RanchoMvc.Data;
using RanchoMvc.Models;

namespace RanchoMvc.Areas.Admin.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        private Cloudinary GetCloudinary()
        {
            var cloudName = System.Configuration.ConfigurationManager.AppSettings["CloudinaryCloudName"];
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["CloudinaryApiKey"];
            var apiSecret = System.Configuration.ConfigurationManager.AppSettings["CloudinaryApiSecret"];
            return new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        }

        public ActionResult Index()
        {
            return View(_db.GalleryImages.OrderBy(g => g.Order).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(HttpPostedFileBase file, string category, string altText)
        {
            if (file == null || file.ContentLength == 0)
            {
                TempData["Error"] = "Selecciona una imagen.";
                return RedirectToAction("Index");
            }

            var cloudName = System.Configuration.ConfigurationManager.AppSettings["CloudinaryCloudName"];
            var apiKey    = System.Configuration.ConfigurationManager.AppSettings["CloudinaryApiKey"];
            var apiSecret = System.Configuration.ConfigurationManager.AppSettings["CloudinaryApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || cloudName == "REEMPLAZAR" ||
                string.IsNullOrEmpty(apiKey)    || apiKey    == "REEMPLAZAR" ||
                string.IsNullOrEmpty(apiSecret) || apiSecret == "REEMPLAZAR")
            {
                TempData["Error"] = "Cloudinary no está configurado. Agrega tus credenciales en Web.config (CloudinaryCloudName, CloudinaryApiKey, CloudinaryApiSecret).";
                return RedirectToAction("Index");
            }

            try
            {
                var cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.InputStream),
                    Folder = "rancho-experience",
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };
                var result = cloudinary.Upload(uploadParams);

                if (result.Error != null)
                {
                    TempData["Error"] = "Cloudinary rechazó la imagen: " + result.Error.Message;
                    return RedirectToAction("Index");
                }

                if (result.SecureUrl == null)
                {
                    TempData["Error"] = "La subida no devolvió una URL. Verifica las credenciales de Cloudinary.";
                    return RedirectToAction("Index");
                }

                var image = new GalleryImage
                {
                    Url = result.SecureUrl.ToString(),
                    PublicId = result.PublicId,
                    Category = category,
                    AltText = altText,
                    Order = _db.GalleryImages.Count() + 1,
                    CreatedAt = DateTime.UtcNow
                };
                _db.GalleryImages.Add(image);
                _db.SaveChanges();
                TempData["Success"] = "Imagen subida correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al subir: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var image = _db.GalleryImages.Find(id);
            if (image != null)
            {
                if (!string.IsNullOrEmpty(image.PublicId))
                {
                    try
                    {
                        GetCloudinary().Destroy(new DeletionParams(image.PublicId));
                    }
                    catch { }
                }
                _db.GalleryImages.Remove(image);
                _db.SaveChanges();
            }
            TempData["Success"] = "Imagen eliminada.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
