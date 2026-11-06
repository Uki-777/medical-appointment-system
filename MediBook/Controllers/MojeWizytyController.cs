using MediBook.Data;
using MediBook.Models;
using MediBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

[Authorize(Roles = "Pacjent")]
public class MojeWizytyController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Uzytkownik> _userManager;

    public MojeWizytyController(
        ApplicationDbContext context,
        UserManager<Uzytkownik> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var pacjent = await PobierzLubUtworzPacjenta();

        if (pacjent == null)
            return NotFound();

        var wizyty = await _context.Wizyty
            .Where(x => x.PacjentId == pacjent.Id)
            .Include(x => x.Lekarz)
                .ThenInclude(x => x.Specjalizacja)
            .Include(x => x.Lekarz)
                .ThenInclude(x => x.PlacowkaMedyczna)
            .OrderBy(x => x.DataWizyty)
            .ToListAsync();

        return View(wizyty);
    }

    public async Task<IActionResult> Umow()
    {
        var pacjent = await PobierzLubUtworzPacjenta();

        if (pacjent == null)
            return NotFound();

        PrzygotujLekarzy();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Umow(
        UmowWizyteViewModel model)
    {
        var pacjent = await PobierzLubUtworzPacjenta();

        if (pacjent == null)
            return NotFound();

        if (model.DataWizyty <= DateTime.Now)
        {
            ModelState.AddModelError(
                "",
                "Nie można umówić wizyty w przeszłości.");
        }

        if (model.DataWizyty.Minute != 0 &&
            model.DataWizyty.Minute != 30)
        {
            ModelState.AddModelError(
                "",
                "Wizyty można umawiać co 30 minut.");
        }

        var data = model.DataWizyty.Date;
        var godzina = model.DataWizyty.TimeOfDay;

        var dostepnyGrafik =
            await _context.GrafikiLekarzy.AnyAsync(x =>
                x.LekarzId == model.LekarzId &&
                x.Data.Date == data &&
                x.CzyDostepny &&
                godzina >= x.GodzinaOd &&
                godzina.Add(TimeSpan.FromMinutes(30)) <= x.GodzinaDo);

        if (!dostepnyGrafik)
        {
            ModelState.AddModelError(
                "",
                "Lekarz nie jest dostępny w wybranym terminie.");
        }

        var poczatekNowejWizyty =
            model.DataWizyty;

        var koniecNowejWizyty =
            model.DataWizyty.AddMinutes(30);

        var zajetyTermin =
            await _context.Wizyty.AnyAsync(x =>
                x.LekarzId == model.LekarzId &&
                x.Status != StatusWizyty.Anulowana &&
                x.DataWizyty < koniecNowejWizyty &&
                x.DataWizyty.AddMinutes(30) > poczatekNowejWizyty);

        if (zajetyTermin)
        {
            ModelState.AddModelError(
                "",
                "Wybrany termin jest już zajęty.");
        }

        if (!ModelState.IsValid)
        {
            PrzygotujLekarzy();

            return View(model);
        }

        var wizyta = new Wizyta
        {
            PacjentId = pacjent.Id,
            LekarzId = model.LekarzId,
            DataWizyty = model.DataWizyty,
            Uwagi = model.Uwagi,
            Status = StatusWizyty.Zaplanowana
        };

        _context.Wizyty.Add(wizyta);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anuluj(int id)
    {
        var pacjent = await PobierzLubUtworzPacjenta();

        if (pacjent == null)
            return NotFound();

        var wizyta =
            await _context.Wizyty.FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.PacjentId == pacjent.Id);

        if (wizyta == null)
            return NotFound();

        if (wizyta.Status == StatusWizyty.Zaplanowana)
        {
            wizyta.Status =
                StatusWizyty.Anulowana;

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private void PrzygotujLekarzy()
    {
        var lekarze =
            _context.Lekarze
                .Include(x => x.Specjalizacja)
                .Include(x => x.PlacowkaMedyczna)
                .ToList()
                .Select(x => new
                {
                    x.Id,
                    Nazwa =
                        x.Imie + " " +
                        x.Nazwisko + " - " +
                        x.Specjalizacja?.Nazwa + " - " +
                        x.PlacowkaMedyczna?.Nazwa
                });

        ViewBag.Lekarze =
            new SelectList(
                lekarze,
                "Id",
                "Nazwa");
    }

    private async Task<Pacjent?> PobierzLubUtworzPacjenta()
    {
        var uzytkownik =
            await _userManager.GetUserAsync(User);

        if (uzytkownik == null)
            return null;

        var pacjent =
            await _context.Pacjenci
                .FirstOrDefaultAsync(x =>
                    x.UzytkownikId == uzytkownik.Id);

        if (pacjent != null)
            return pacjent;

        pacjent = new Pacjent
        {
            Imie = uzytkownik.Imie,
            Nazwisko = uzytkownik.Nazwisko,
            Email = uzytkownik.Email ?? string.Empty,
            UzytkownikId = uzytkownik.Id
        };

        _context.Pacjenci.Add(pacjent);

        await _context.SaveChangesAsync();

        return pacjent;
    }
}