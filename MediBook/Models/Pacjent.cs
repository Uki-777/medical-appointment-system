using System.ComponentModel.DataAnnotations;

namespace MediBook.Models;

public class Pacjent
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Imie { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Nazwisko { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? NumerTelefonu { get; set; }

    public DateTime? DataUrodzenia { get; set; }

    public ICollection<Wizyta> Wizyty { get; set; } = new List<Wizyta>();
}