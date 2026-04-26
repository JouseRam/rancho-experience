using System;
using System.ComponentModel.DataAnnotations;

namespace RanchoMvc.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required, MaxLength(200), Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required, EmailAddress, MaxLength(200), Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [MaxLength(20), Display(Name = "Teléfono")]
        public string Phone { get; set; }

        [MaxLength(200), Display(Name = "Empresa")]
        public string Company { get; set; }

        [Required, Display(Name = "Mensaje")]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
