using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

public class WizytyController : Controller
{
    private readonly ApplicationDbContext _context;

    public WizytyController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var wizyty = await _context.Wizyty
            .Include(x => x.Pacjent)
            .Include(x => x.Lekarz)
            .ThenInclude(x => x.Specjalizacja)
            .Include(x => x.Lekarz)
            .ThenInclude(x => x.PlacowkaMedyczna)
            .OrderBy(x => x.DataWizyty)
            .ToListAsync();

        return View(wizyty);
    }

    public IActionResult Create()
    {
        PrzygotujListy();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Wizyta wizyta)
    {
        var zajetyTermin =
            await _context.Wizyty.AnyAsync(x =>
                x.LekarzId == wizyta.LekarzId &&
                x.DataWizyty == wizyta.DataWizyty &&
                x.Status != StatusWizyty.Anulowana);

        if (zajetyTermin)
        {
            ModelState.AddModelError(
                "",
                "Wybrany termin jest już zajęty.");
        }

        var data = wizyta.DataWizyty.Date;
        var godzina = wizyta.DataWizyty.TimeOfDay;

        var dostepnyGrafik =
            await _context.GrafikiLekarzy.AnyAsync(x =>
                x.LekarzId == wizyta.LekarzId &&
                x.Data.Date == data &&
                x.CzyDostepny &&
                godzina >= x.GodzinaOd &&
                godzina < x.GodzinaDo);

        if (!dostepnyGrafik)
        {
            ModelState.AddModelError(
                "",
                "Lekarz nie jest dostępny w wybranym terminie.");
        }

        if (wizyta.DataWizyty <= DateTime.Now)
        {
            ModelState.AddModelError(
                "",
                "Nie można umówić wizyty w przeszłości.");
        }

        if (!ModelState.IsValid)
        {
            PrzygotujListy();

            return View(wizyta);
        }

        wizyta.Status =
            StatusWizyty.Zaplanowana;

        _context.Wizyty.Add(wizyta);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anuluj(int id)
    {
        var wizyta =
            await _context.Wizyty.FindAsync(id);

        if (wizyta != null)
        {
            wizyta.Status =
                StatusWizyty.Anulowana;

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Zakoncz(int id)
    {
        var wizyta =
            await _context.Wizyty.FindAsync(id);

        if (wizyta != null)
        {
            wizyta.Status =
                StatusWizyty.Zakonczona;

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private void PrzygotujListy()
    {
        ViewBag.Pacjenci =
            new SelectList(
                _context.Pacjenci.Select(x => new
                {
                    x.Id,
                    Nazwa =
                        x.Imie + " " + x.Nazwisko
                }),
                "Id",
                "Nazwa");

        ViewBag.Lekarze =
            new SelectList(
                _context.Lekarze
                    .Include(x => x.Specjalizacja)
                    .Select(x => new
                    {
                        x.Id,
                        Nazwa =
                            x.Imie + " "
                            + x.Nazwisko + " - "
                            + x.Specjalizacja!.Nazwa
                    }),
                "Id",
                "Nazwa");
    }
}