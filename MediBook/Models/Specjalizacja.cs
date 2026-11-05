using System.ComponentModel.DataAnnotations;

namespace MediBook.Models;

public class Specjalizacja
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nazwa { get; set; } = string.Empty;

    public ICollection<Lekarz> Lekarze { get; set; } = new List<Lekarz>();
}