using MediBook.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Data;

public class ApplicationDbContext
    : IdentityDbContext<Uzytkownik>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pacjent>
        Pacjenci { get; set; }

    public DbSet<Lekarz>
        Lekarze { get; set; }

    public DbSet<PlacowkaMedyczna>
        PlacowkiMedyczne { get; set; }

    public DbSet<Specjalizacja>
        Specjalizacje { get; set; }

    public DbSet<Wizyta>
        Wizyty { get; set; }

    public DbSet<GrafikLekarza>
        GrafikiLekarzy { get; set; }

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Pacjent>()
            .HasIndex(x => x.Email)
            .IsUnique();

        builder.Entity<Pacjent>()
            .HasIndex(x => x.UzytkownikId)
            .IsUnique()
            .HasFilter(
                "[UzytkownikId] IS NOT NULL");

        builder.Entity<Lekarz>()
            .HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter(
                "[Email] IS NOT NULL");

        builder.Entity<Lekarz>()
            .HasIndex(x => x.UzytkownikId)
            .IsUnique()
            .HasFilter(
                "[UzytkownikId] IS NOT NULL");
    }
}