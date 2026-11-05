using System.ComponentModel.DataAnnotations;

namespace MediBook.Models;

public class GrafikLekarza
{
    public int Id { get; set; }

    [Required]
    public int LekarzId { get; set; }

    public Lekarz? Lekarz { get; set; }

    [Required]
    public DateTime Data { get; set; }

    [Required]
    public TimeSpan GodzinaOd { get; set; }

    [Required]
    public TimeSpan GodzinaDo { get; set; }

    public bool CzyDostepny { get; set; } = true;
}