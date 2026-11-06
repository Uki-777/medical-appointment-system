using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels;

public class RejestracjaViewModel
{
    [Required]
    public string Imie { get; set; } = string.Empty;

    [Required]
    public string Nazwisko { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Haslo { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Haslo")]
    public string PowtorzHaslo { get; set; } = string.Empty;
}