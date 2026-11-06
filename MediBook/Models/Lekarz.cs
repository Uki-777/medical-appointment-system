using System.ComponentModel.DataAnnotations;

namespace MediBook.Models;

public class Lekarz
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Imie { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nazwisko { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Email { get; set; }

    public string? UzytkownikId { get; set; }

    public Uzytkownik? Uzytkownik { get; set; }

    [Required]
    public int SpecjalizacjaId { get; set; }

    public Specjalizacja? Specjalizacja { get; set; }

    [Required]
    public int PlacowkaMedycznaId { get; set; }

    public PlacowkaMedyczna? PlacowkaMedyczna { get; set; }

    public ICollection<Wizyta> Wizyty { get; set; } = new List<Wizyta>();

    public ICollection<GrafikLekarza> Grafiki { get; set; } = new List<GrafikLekarza>();
}