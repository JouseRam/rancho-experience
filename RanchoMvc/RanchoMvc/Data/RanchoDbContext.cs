using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using RanchoMvc.Models;

namespace RanchoMvc.Data
{
    public class RanchoDbContext : IdentityDbContext<ApplicationUser>
    {
        public RanchoDbContext() : base("RanchoDb", throwIfV1Schema: false) { }

        public static RanchoDbContext Create() => new RanchoDbContext();

        public DbSet<Plan> Plans { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plan>().ToTable("Plans");
            modelBuilder.Entity<GalleryImage>().ToTable("GalleryImages");
            modelBuilder.Entity<ContactMessage>().ToTable("ContactMessages");
            modelBuilder.Entity<Reservation>().ToTable("Reservations");
            modelBuilder.Entity<SiteSetting>().ToTable("SiteSettings");
        }
    }
}
