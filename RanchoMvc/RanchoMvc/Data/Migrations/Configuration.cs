using System;
using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RanchoMvc.Models;

namespace RanchoMvc.Data.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<RanchoDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "RanchoMvc.Data.RanchoDbContext";
        }

        protected override void Seed(RanchoDbContext context)
        {
            SeedAdmin(context);
            SeedPlans(context);
            SeedSiteSettings(context);
        }

        private void SeedAdmin(RanchoDbContext context)
        {
            var adminEmail = System.Configuration.ConfigurationManager.AppSettings["AdminEmail"] ?? "admin@rancho.com";
            var adminPassword = System.Configuration.ConfigurationManager.AppSettings["AdminPassword"] ?? "Rancho2025!";

            if (context.Users.Any(u => u.Email == adminEmail)) return;

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var user = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = userManager.Create(user, adminPassword);
            if (!result.Succeeded)
                throw new Exception("Error creando admin: " + string.Join(", ", result.Errors));
        }

        private void SeedPlans(RanchoDbContext context)
        {
            if (context.Plans.Any()) return;

            context.Plans.AddOrUpdate(p => p.Title,
                new Plan
                {
                    Title = "Paseo a Caballo al Atardecer",
                    ShortDescription = "Descubre la libertad de recorrer el campo a caballo",
                    Description = "Descubre la libertad de recorrer el campo a caballo mientras el sol pinta el cielo de colores únicos. Una experiencia que conecta tu espíritu con la naturaleza. Con guías expertos, esta cabalgata está diseñada para toda la familia que busca una aventura auténtica.",
                    Price = 800,
                    PriceLabel = "desde $800 MXN / persona",
                    Icon = "fa-solid fa-horse",
                    IsActive = true,
                    Order = 1,
                    Includes = "Guía experto certificado|Equipo de seguridad completo|Fotografías del recorrido|Bebida de bienvenida"
                },
                new Plan
                {
                    Title = "Noches de Fogata",
                    ShortDescription = "Bajo un cielo estrellado, comparte historias alrededor del fuego",
                    Description = "Bajo un cielo estrellado, comparte historias alrededor del fuego. Momentos mágicos que crean memorias para toda la vida. Ideal para reuniones familiares, celebraciones y convivencias en un ambiente único e irrepetible.",
                    Price = 1500,
                    PriceLabel = "desde $1,500 MXN / persona",
                    Icon = "fa-solid fa-fire",
                    IsActive = true,
                    Order = 2,
                    Includes = "Fogata y s'mores|Cena tradicional bajo las estrellas|Actividades de integración|Hospedaje disponible"
                },
                new Plan
                {
                    Title = "Descanso Total",
                    ShortDescription = "Desconéctate del mundo y sumérgete en la tranquilidad del rancho",
                    Description = "Desconéctate del mundo y sumérgete en la tranquilidad del rancho. Aquí, cada respiración es un paso hacia la paz interior. Un retiro diseñado para reconectar a toda tu familia con lo esencial: la naturaleza, el silencio y el bienestar.",
                    Price = 2500,
                    PriceLabel = "desde $2,500 MXN / persona",
                    Icon = "fa-solid fa-sun",
                    IsActive = true,
                    Order = 3,
                    Includes = "Alojamiento rústico de lujo|Desayuno y cena incluidos|Actividades de relajación|Acceso a todas las instalaciones"
                }
            );

            context.SaveChanges();
        }

        private void EnsureSetting(RanchoDbContext context, string key, string label, string type, string group, string value = "")
        {
            if (!context.SiteSettings.Any(s => s.Key == key))
            {
                context.SiteSettings.Add(new SiteSetting { Key = key, Label = label, Type = type, Group = group, Value = value });
            }
        }

        private void SeedSiteSettings(RanchoDbContext context)
        {
            if (!context.SiteSettings.Any())
            {
                // Instalación nueva — insertar todo de una vez
                var settings = new[]
                {
                // Hero
                new SiteSetting { Key = "HeroVideoUrl", Label = "Video de fondo (Hero)", Type = "video-url", Group = "Hero",
                    Value = "" },
                new SiteSetting { Key = "HeroImageUrl", Label = "Imagen de fondo (Hero / respaldo)", Type = "image-url", Group = "Hero",
                    Value = "https://images.unsplash.com/photo-1500534314209-a25ddb2bd429?w=1920&q=80" },
                new SiteSetting { Key = "HeroTitle", Label = "Título principal", Type = "text", Group = "Hero",
                    Value = "Vive la Naturaleza en Rancho El Pato" },
                new SiteSetting { Key = "HeroSubtitle", Label = "Subtítulo", Type = "textarea", Group = "Hero",
                    Value = "Experiencias únicas que conectan tu espíritu con la tierra. Desde cabalgatas al atardecer hasta noches de fogata bajo las estrellas." },
                new SiteSetting { Key = "HeroCtaText", Label = "Texto del botón CTA", Type = "text", Group = "Hero",
                    Value = "Reserva Tu Experiencia" },

                // About
                new SiteSetting { Key = "AboutTitle", Label = "Título sección About", Type = "text", Group = "About",
                    Value = "Un Refugio en la Naturaleza" },
                new SiteSetting { Key = "AboutText", Label = "Texto descriptivo", Type = "textarea", Group = "About",
                    Value = "Más que un destino, es una experiencia que conecta el alma con la tierra. Aquí, el tiempo se detiene y la naturaleza te abraza.\n\nDesde el amanecer hasta el atardecer, cada momento está diseñado para liberarte del estrés y reconectarte con lo esencial: la tranquilidad, la aventura y la belleza del campo auténtico." },
                new SiteSetting { Key = "AboutImageUrl", Label = "Imagen sección About", Type = "image-url", Group = "About",
                    Value = "https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?w=900&q=80" },

                // CTA Section
                new SiteSetting { Key = "CtaBackgroundUrl", Label = "Imagen/Video de fondo CTA", Type = "image-url", Group = "CTA",
                    Value = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1920&q=80" },
                new SiteSetting { Key = "CtaTitle", Label = "Título CTA", Type = "text", Group = "CTA",
                    Value = "¿Listo para la Aventura?" },
                new SiteSetting { Key = "CtaSubtitle", Label = "Subtítulo CTA", Type = "textarea", Group = "CTA",
                    Value = "Reserva tu experiencia y descubre un mundo donde la naturaleza y la tranquilidad se encuentran." },

                // Contact
                new SiteSetting { Key = "ContactPhone", Label = "Teléfono", Type = "text", Group = "Contacto",
                    Value = "+52 123 456 7890" },
                new SiteSetting { Key = "ContactEmail", Label = "Correo de contacto", Type = "text", Group = "Contacto",
                    Value = "info@ranchoelpato.com" },
                new SiteSetting { Key = "ContactAddress", Label = "Dirección", Type = "text", Group = "Contacto",
                    Value = "Camino al Rancho, México" },
                new SiteSetting { Key = "BusinessHours", Label = "Horarios", Type = "text", Group = "Contacto",
                    Value = "Abierto todos los días · 9:00 AM – 6:00 PM" },
                new SiteSetting { Key = "BusinessName", Label = "Nombre del rancho", Type = "text", Group = "General",
                    Value = "Rancho El Pato" },
                new SiteSetting { Key = "LogoUrl", Label = "Logo del Rancho", Type = "image-url", Group = "General",
                    Value = "" },
                new SiteSetting { Key = "SocialInstagram", Label = "Instagram URL", Type = "url", Group = "Redes",
                    Value = "" },
                new SiteSetting { Key = "SocialFacebook", Label = "Facebook URL", Type = "url", Group = "Redes",
                    Value = "" },
                new SiteSetting { Key = "SocialWhatsapp", Label = "WhatsApp (número)", Type = "text", Group = "Redes",
                    Value = "5212345678" },
                };
                context.SiteSettings.AddRange(settings);
                context.SaveChanges();
            }
            else
            {
                // BD existente — agregar solo los settings que falten
                EnsureSetting(context, "LogoUrl",         "Logo del Rancho",          "image-url",    "General");
                EnsureSetting(context, "ColorPrimary",    "Color principal (verde)",  "color",        "Apariencia", "#2D5016");
                EnsureSetting(context, "ColorSecondary",  "Color dorado / acentos",   "color",        "Apariencia", "#C4882A");
                EnsureSetting(context, "ColorAccent",     "Color café / detalles",    "color",        "Apariencia", "#5C3A1E");
                EnsureSetting(context, "FontTitle",       "Fuente de títulos",        "font-select",  "Apariencia", "Playfair Display");
                EnsureSetting(context, "FontBody",        "Fuente de texto",          "font-select",  "Apariencia", "Inter");
                EnsureSetting(context, "BankName",        "Banco",                    "text",         "Pagos");
                EnsureSetting(context, "BankHolder",      "Titular de la cuenta",     "text",         "Pagos");
                EnsureSetting(context, "BankAccount",     "Número de cuenta",         "text",         "Pagos");
                EnsureSetting(context, "BankCLABE",       "CLABE interbancaria",      "text",         "Pagos");
                context.SaveChanges();
            }
        }
    }
}
