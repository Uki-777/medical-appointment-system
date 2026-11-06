using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels;

public class LogowanieViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Haslo { get; set; } = string.Empty;

    public bool ZapamietajMnie { get; set; }
}