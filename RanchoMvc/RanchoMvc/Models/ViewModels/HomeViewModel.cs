using System.Collections.Generic;

namespace RanchoMvc.Models.ViewModels
{
    public class HomeViewModel
    {
        public IList<Plan> Plans { get; set; }
        public IList<GalleryImage> GalleryImages { get; set; }
        public ReservationViewModel ReservationForm { get; set; }
        public ContactViewModel ContactForm { get; set; }
        public Dictionary<string, string> Settings { get; set; }

        public string Setting(string key, string fallback = "")
        {
            if (Settings != null && Settings.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val))
                return val;
            return fallback;
        }
    }
}
