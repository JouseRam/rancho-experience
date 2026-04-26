using System;
using System.ComponentModel.DataAnnotations;

namespace RanchoMvc.Models
{
    public class GalleryImage
    {
        public int Id { get; set; }

        [Required]
        public string Url { get; set; }

        public string PublicId { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        [MaxLength(200)]
        public string AltText { get; set; }

        public int Order { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
