using System.ComponentModel.DataAnnotations;

namespace MediBook.Models;

public class PlacowkaMedyczna
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nazwa { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Adres { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Miasto { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? NumerTelefonu { get; set; }

    public ICollection<Lekarz> Lekarze { get; set; } = new List<Lekarz>();
}