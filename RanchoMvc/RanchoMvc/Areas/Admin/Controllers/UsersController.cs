using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using RanchoMvc.Data;
using RanchoMvc.Filters;
using RanchoMvc.Models;
using RanchoMvc.Models.ViewModels;

namespace RanchoMvc.Areas.Admin.Controllers
{
    [ModuleAuthorize(SuperAdminOnly = true)]
    public class UsersController : BaseAdminController
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        private UserManager<ApplicationUser> GetUserManager()
        {
            return new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Usuarios";
            var users = _db.Users.OrderBy(u => u.Email).ToList();
            var allPerms = _db.UserModulePermissions.ToList();

            var currentUserId = User.Identity.GetUserId();
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.AllPerms = allPerms;
            return View(users);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Title = "Nuevo Usuario";
            return View("CreateEdit", new UserEditViewModel { IsNew = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserEditViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError("Password", "La contraseña es requerida para nuevos usuarios.");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Nuevo Usuario";
                vm.IsNew = true;
                return View("CreateEdit", vm);
            }

            var manager = GetUserManager();
            if (manager.FindByEmail(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con ese correo.");
                ViewBag.Title = "Nuevo Usuario";
                vm.IsNew = true;
                return View("CreateEdit", vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,
                IsSuperAdmin = vm.IsSuperAdmin
            };

            var result = manager.Create(user, vm.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", string.Join(" ", result.Errors));
                ViewBag.Title = "Nuevo Usuario";
                vm.IsNew = true;
                return View("CreateEdit", vm);
            }

            if (!vm.IsSuperAdmin)
                SavePermissions(user.Id, vm);

            TempData["Success"] = "Usuario creado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            var user = _db.Users.Find(id);
            if (user == null) return HttpNotFound();

            ViewBag.Title = "Editar Usuario";
            ViewBag.IsCurrentUser = (id == User.Identity.GetUserId());
            return View("CreateEdit", BuildViewModel(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserEditViewModel vm)
        {
            // Password is optional on edit — remove validation if empty
            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Editar Usuario";
                ViewBag.IsCurrentUser = (vm.Id == User.Identity.GetUserId());
                return View("CreateEdit", vm);
            }

            var user = _db.Users.Find(vm.Id);
            if (user == null) return HttpNotFound();

            user.IsSuperAdmin = vm.IsSuperAdmin;
            _db.SaveChanges();

            // Change password if provided
            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var manager = GetUserManager();
                manager.RemovePassword(vm.Id);
                var passResult = manager.AddPassword(vm.Id, vm.Password);
                if (!passResult.Succeeded)
                {
                    ModelState.AddModelError("Password", string.Join(" ", passResult.Errors));
                    ViewBag.Title = "Editar Usuario";
                    return View("CreateEdit", vm);
                }
            }

            // Update permissions
            SavePermissions(vm.Id, vm);

            TempData["Success"] = "Usuario actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            if (id == User.Identity.GetUserId())
            {
                TempData["Error"] = "No puedes eliminarte a ti mismo.";
                return RedirectToAction("Index");
            }

            var user = _db.Users.Find(id);
            if (user == null) return HttpNotFound();

            // Remove permissions first (cascade should handle it, but be explicit)
            var perms = _db.UserModulePermissions.Where(p => p.UserId == id).ToList();
            _db.UserModulePermissions.RemoveRange(perms);
            _db.SaveChanges();

            var manager = GetUserManager();
            manager.Delete(user);

            TempData["Success"] = "Usuario eliminado.";
            return RedirectToAction("Index");
        }

        // ---- Helpers ----

        private UserEditViewModel BuildViewModel(ApplicationUser user)
        {
            var perms = _db.UserModulePermissions.Where(p => p.UserId == user.Id).ToList();
            Func<string, bool> view = m => perms.Any(p => p.Module == m && p.CanView);
            Func<string, bool> edit = m => perms.Any(p => p.Module == m && p.CanEdit);

            return new UserEditViewModel
            {
                Id              = user.Id,
                Email           = user.Email,
                IsSuperAdmin    = user.IsSuperAdmin,
                IsNew           = false,
                PlansView        = view(AdminModule.Plans),
                PlansEdit        = edit(AdminModule.Plans),
                ReservationsView = view(AdminModule.Reservations),
                ReservationsEdit = edit(AdminModule.Reservations),
                MessagesView     = view(AdminModule.Messages),
                MessagesEdit     = edit(AdminModule.Messages),
                GalleryView      = view(AdminModule.Gallery),
                GalleryEdit      = edit(AdminModule.Gallery),
                SettingsView     = view(AdminModule.Settings),
                SettingsEdit     = edit(AdminModule.Settings),
            };
        }

        private void SavePermissions(string userId, UserEditViewModel vm)
        {
            var existing = _db.UserModulePermissions.Where(p => p.UserId == userId).ToList();
            _db.UserModulePermissions.RemoveRange(existing);

            // If edit implies view, enforce it
            if (vm.PlansEdit)        vm.PlansView        = true;
            if (vm.ReservationsEdit) vm.ReservationsView = true;
            if (vm.MessagesEdit)     vm.MessagesView     = true;
            if (vm.GalleryEdit)      vm.GalleryView      = true;
            if (vm.SettingsEdit)     vm.SettingsView     = true;

            var toAdd = new[]
            {
                new UserModulePermission { UserId = userId, Module = AdminModule.Plans,        CanView = vm.PlansView,        CanEdit = vm.PlansEdit },
                new UserModulePermission { UserId = userId, Module = AdminModule.Reservations, CanView = vm.ReservationsView, CanEdit = vm.ReservationsEdit },
                new UserModulePermission { UserId = userId, Module = AdminModule.Messages,     CanView = vm.MessagesView,     CanEdit = vm.MessagesEdit },
                new UserModulePermission { UserId = userId, Module = AdminModule.Gallery,      CanView = vm.GalleryView,      CanEdit = vm.GalleryEdit },
                new UserModulePermission { UserId = userId, Module = AdminModule.Settings,     CanView = vm.SettingsView,     CanEdit = vm.SettingsEdit },
            };

            foreach (var p in toAdd)
            {
                if (p.CanView || p.CanEdit)
                    _db.UserModulePermissions.Add(p);
            }

            _db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
