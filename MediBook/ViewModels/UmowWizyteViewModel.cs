using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels;

public class UmowWizyteViewModel
{
    [Required]
    public int LekarzId { get; set; }

    [Required]
    public DateTime DataWizyty { get; set; }

    [MaxLength(500)]
    public string? Uwagi { get; set; }
}