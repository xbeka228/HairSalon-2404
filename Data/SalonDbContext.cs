using Microsoft.EntityFrameworkCore;
using HairSalon.Models;

namespace HairSalon.Data;

public class SalonDbContext : DbContext
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Master> Masters => Set<Master>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dbPath = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "salon.db");
        options.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Client)
            .WithMany()
            .HasForeignKey("ClientId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Master)
            .WithMany()
            .HasForeignKey("MasterId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany()
            .HasForeignKey("ServiceId")
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .Ignore(a => a.Summary);
    }
}
