using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

[Authorize(Roles = "Administrator")]
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
                .ThenInclude(x => x!.Specjalizacja)
            .Include(x => x.Lekarz)
                .ThenInclude(x => x!.PlacowkaMedyczna)
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
        var pacjentIstnieje =
            await _context.Pacjenci.AnyAsync(x =>
                x.Id == wizyta.PacjentId);

        if (!pacjentIstnieje)
        {
            ModelState.AddModelError(
                "PacjentId",
                "Wybrany pacjent nie istnieje.");
        }

        var lekarzIstnieje =
            await _context.Lekarze.AnyAsync(x =>
                x.Id == wizyta.LekarzId);

        if (!lekarzIstnieje)
        {
            ModelState.AddModelError(
                "LekarzId",
                "Wybrany lekarz nie istnieje.");
        }

        if (wizyta.DataWizyty <= DateTime.Now)
        {
            ModelState.AddModelError(
                "",
                "Nie można umówić wizyty w przeszłości.");
        }

        if (wizyta.DataWizyty.Minute != 0 &&
            wizyta.DataWizyty.Minute != 30)
        {
            ModelState.AddModelError(
                "",
                "Wizyty można umawiać co 30 minut.");
        }

        var data =
            wizyta.DataWizyty.Date;

        var godzina =
            wizyta.DataWizyty.TimeOfDay;

        var dostepnyGrafik =
            await _context.GrafikiLekarzy.AnyAsync(x =>
                x.LekarzId == wizyta.LekarzId &&
                x.Data.Date == data &&
                x.CzyDostepny &&
                godzina >= x.GodzinaOd &&
                godzina.Add(
                    TimeSpan.FromMinutes(30)) <= x.GodzinaDo);

        if (!dostepnyGrafik)
        {
            ModelState.AddModelError(
                "",
                "Lekarz nie jest dostępny w wybranym terminie.");
        }

        var koniecNowejWizyty =
            wizyta.DataWizyty.AddMinutes(30);

        var zajetyTermin =
            await _context.Wizyty.AnyAsync(x =>
                x.LekarzId == wizyta.LekarzId &&
                x.Status != StatusWizyty.Anulowana &&
                x.DataWizyty < koniecNowejWizyty &&
                x.DataWizyty.AddMinutes(30) >
                wizyta.DataWizyty);

        if (zajetyTermin)
        {
            ModelState.AddModelError(
                "",
                "Wybrany termin jest już zajęty.");
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

        if (wizyta == null)
            return NotFound();

        if (wizyta.Status ==
            StatusWizyty.Zaplanowana)
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

        if (wizyta == null)
            return NotFound();

        if (wizyta.Status ==
            StatusWizyty.Zaplanowana)
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
                _context.Pacjenci
                    .OrderBy(x => x.Nazwisko)
                    .ThenBy(x => x.Imie)
                    .Select(x => new
                    {
                        x.Id,
                        Nazwa =
                            x.Imie + " " +
                            x.Nazwisko
                    }),
                "Id",
                "Nazwa");

        ViewBag.Lekarze =
            new SelectList(
                _context.Lekarze
                    .Include(x => x.Specjalizacja)
                    .OrderBy(x => x.Nazwisko)
                    .ThenBy(x => x.Imie)
                    .Select(x => new
                    {
                        x.Id,
                        Nazwa =
                            x.Imie + " " +
                            x.Nazwisko + " - " +
                            x.Specjalizacja!.Nazwa
                    }),
                "Id",
                "Nazwa");
    }
}