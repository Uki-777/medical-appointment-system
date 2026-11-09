using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels;

public class DodajLekarzaViewModel
{
    [Required(
        ErrorMessage = "Imię jest wymagane.")]
    [MaxLength(100)]
    public string Imie { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage = "Nazwisko jest wymagane.")]
    [MaxLength(100)]
    public string Nazwisko { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage = "Email jest wymagany.")]
    [EmailAddress(
        ErrorMessage = "Podaj poprawny adres email.")]
    [MaxLength(150)]
    public string Email { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage = "Wybierz specjalizację.")]
    public int SpecjalizacjaId { get; set; }

    [Required(
        ErrorMessage = "Wybierz placówkę.")]
    public int PlacowkaMedycznaId { get; set; }

    [Required(
        ErrorMessage = "Hasło jest wymagane.")]
    [MinLength(
        6,
        ErrorMessage =
            "Hasło musi mieć co najmniej 6 znaków.")]
    [DataType(DataType.Password)]
    public string HasloStartowe { get; set; } =
        string.Empty;
}