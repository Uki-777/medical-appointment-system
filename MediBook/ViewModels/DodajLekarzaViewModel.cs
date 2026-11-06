using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels;

public class DodajLekarzaViewModel
{
    [Required]
    public string Imie { get; set; } = string.Empty;

    [Required]
    public string Nazwisko { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public int SpecjalizacjaId { get; set; }

    [Required]
    public int PlacowkaMedycznaId { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string HasloStartowe { get; set; } = string.Empty;
}