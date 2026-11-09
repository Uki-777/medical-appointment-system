using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels;

public class RejestracjaViewModel
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
        ErrorMessage =
            "Podaj poprawny adres email.")]
    [MaxLength(150)]
    public string Email { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage = "Hasło jest wymagane.")]
    [MinLength(
        6,
        ErrorMessage =
            "Hasło musi mieć co najmniej 6 znaków.")]
    [DataType(DataType.Password)]
    public string Haslo { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage =
            "Powtórzenie hasła jest wymagane.")]
    [DataType(DataType.Password)]
    [Compare(
        "Haslo",
        ErrorMessage =
            "Podane hasła nie są identyczne.")]
    public string PowtorzHaslo { get; set; } =
        string.Empty;
}