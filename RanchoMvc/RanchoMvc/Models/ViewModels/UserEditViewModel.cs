using System.ComponentModel.DataAnnotations;

namespace RanchoMvc.Models.ViewModels
{
    public class UserEditViewModel
    {
        public string Id    { get; set; }
        public bool   IsNew { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Super Administrador")]
        public bool IsSuperAdmin { get; set; }

        // -- Permissions per module (flat, easy form binding) --
        public bool PlansView        { get; set; }
        public bool PlansEdit        { get; set; }

        public bool ReservationsView { get; set; }
        public bool ReservationsEdit { get; set; }

        public bool MessagesView     { get; set; }
        public bool MessagesEdit     { get; set; }

        public bool GalleryView      { get; set; }
        public bool GalleryEdit      { get; set; }

        public bool SettingsView     { get; set; }
        public bool SettingsEdit     { get; set; }
    }
}
