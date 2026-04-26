using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RanchoMvc.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required, MaxLength(200), Display(Name = "Empresa")]
        public string CompanyName { get; set; }

        [Required, MaxLength(200), Display(Name = "Nombre de contacto")]
        public string ContactName { get; set; }

        [Required, EmailAddress, MaxLength(200), Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [MaxLength(20), Display(Name = "Teléfono")]
        public string Phone { get; set; }

        [Display(Name = "Plan de interés")]
        public int? PlanId { get; set; }

        [ForeignKey("PlanId")]
        public virtual Plan Plan { get; set; }

        [DataType(DataType.Date), Display(Name = "Fecha preferida")]
        public DateTime? PreferredDate { get; set; }

        [Range(1, 500), Display(Name = "Número de personas")]
        public int GuestCount { get; set; } = 1;

        [MaxLength(50)]
        public string Status { get; set; } = "Pendiente";

        [Display(Name = "Notas adicionales")]
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
