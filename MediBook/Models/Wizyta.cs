using System.ComponentModel.DataAnnotations;

namespace MediBook.Models;

public class Wizyta
{
    public int Id { get; set; }

    [Required]
    public int PacjentId { get; set; }

    public Pacjent? Pacjent { get; set; }

    [Required]
    public int LekarzId { get; set; }

    public Lekarz? Lekarz { get; set; }

    [Required]
    public DateTime DataWizyty { get; set; }

    public StatusWizyty Status { get; set; } = StatusWizyty.Zaplanowana;

    [MaxLength(500)]
    public string? Uwagi { get; set; }
}