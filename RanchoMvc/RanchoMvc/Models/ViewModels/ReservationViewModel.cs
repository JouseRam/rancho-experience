using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace RanchoMvc.Models.ViewModels
{
    public class ReservationViewModel
    {
        [Required(ErrorMessage = "El nombre de la empresa es requerido")]
        [Display(Name = "Empresa")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "El nombre de contacto es requerido")]
        [Display(Name = "Nombre de contacto")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Display(Name = "Teléfono")]
        public string Phone { get; set; }

        [Display(Name = "Plan de interés")]
        public int? PlanId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha preferida")]
        public DateTime? PreferredDate { get; set; }

        [Range(1, 500, ErrorMessage = "Entre 1 y 500 personas")]
        [Display(Name = "Número de personas")]
        public int GuestCount { get; set; } = 1;

        [Display(Name = "Notas adicionales")]
        public string Notes { get; set; }

        public IEnumerable<SelectListItem> Plans { get; set; }
    }
}
