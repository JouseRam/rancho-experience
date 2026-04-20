using Microsoft.EntityFrameworkCore;
using RanchoApi.Models;

namespace RanchoApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<GalleryImage> GalleryImages => Set<GalleryImage>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
}
