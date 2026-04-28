using System.Collections.Generic;

namespace RanchoMvc.Models
{
    public static class AdminModule
    {
        public const string Plans        = "Plans";
        public const string Reservations = "Reservations";
        public const string Messages     = "Messages";
        public const string Gallery      = "Gallery";
        public const string Settings     = "Settings";

        public static readonly IReadOnlyList<ModuleInfo> All = new List<ModuleInfo>
        {
            new ModuleInfo { Key = Plans,        Label = "Planes",           Icon = "bi-card-list" },
            new ModuleInfo { Key = Reservations, Label = "Reservaciones",    Icon = "bi-calendar-check" },
            new ModuleInfo { Key = Messages,     Label = "Mensajes",         Icon = "bi-envelope" },
            new ModuleInfo { Key = Gallery,      Label = "Galería",          Icon = "bi-images" },
            new ModuleInfo { Key = Settings,     Label = "Ajustes del Sitio",Icon = "bi-sliders" },
        };
    }

    public class ModuleInfo
    {
        public string Key   { get; set; }
        public string Label { get; set; }
        public string Icon  { get; set; }
    }
}
