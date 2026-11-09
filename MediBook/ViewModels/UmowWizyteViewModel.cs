using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels;

public class UmowWizyteViewModel
{
    [Required]
    public int LekarzId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Data { get; set; }

    [Required]
    public string Godzina { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Uwagi { get; set; }
}