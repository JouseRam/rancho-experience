using System.ComponentModel.DataAnnotations;

namespace RanchoMvc.Models
{
    public class SiteSetting
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Key { get; set; }

        public string Value { get; set; }

        [MaxLength(200)]
        public string Label { get; set; }

        // "text" | "textarea" | "url" | "video-url" | "image-url"
        [MaxLength(20)]
        public string Type { get; set; } = "text";

        [MaxLength(100)]
        public string Group { get; set; } = "General";
    }
}
