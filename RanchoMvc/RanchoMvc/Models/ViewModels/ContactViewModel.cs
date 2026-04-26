using System.ComponentModel.DataAnnotations;

namespace RanchoMvc.Models.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Display(Name = "Teléfono")]
        public string Phone { get; set; }

        [Display(Name = "Empresa")]
        public string Company { get; set; }

        [Required(ErrorMessage = "El mensaje es requerido")]
        [Display(Name = "Mensaje")]
        public string Message { get; set; }
    }
}
