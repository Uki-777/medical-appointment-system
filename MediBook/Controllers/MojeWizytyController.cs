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
        var pacjent =
            await PobierzLubUtworzPacjenta();

        if (pacjent == null)
            return NotFound();

        var wizyty =
            await _context.Wizyty
                .Where(x =>
                    x.PacjentId == pacjent.Id)
                .Include(x => x.Lekarz)
                    .ThenInclude(x => x!.Specjalizacja)
                .Include(x => x.Lekarz)
                    .ThenInclude(x => x!.PlacowkaMedyczna)
                .OrderBy(x => x.DataWizyty)
                .ToListAsync();

        return View(wizyty);
    }

    public async Task<IActionResult> Umow()
    {
        var pacjent =
            await PobierzLubUtworzPacjenta();

        if (pacjent == null)
            return NotFound();

        PrzygotujLekarzy();

        return View(
            new UmowWizyteViewModel
            {
                Data = DateTime.Today
            });
    }

    [HttpGet]
    public async Task<IActionResult>
        PobierzDostepneGodziny(
            int lekarzId,
            DateTime data)
    {
        if (lekarzId <= 0 ||
            data.Date < DateTime.Today)
        {
            return Json(
                Array.Empty<string>());
        }

        var lekarzIstnieje =
            await _context.Lekarze.AnyAsync(x =>
                x.Id == lekarzId);

        if (!lekarzIstnieje)
        {
            return Json(
                Array.Empty<string>());
        }

        var grafiki =
            await _context.GrafikiLekarzy
                .Where(x =>
                    x.LekarzId == lekarzId &&
                    x.Data.Date == data.Date &&
                    x.CzyDostepny)
                .OrderBy(x => x.GodzinaOd)
                .ToListAsync();

        if (!grafiki.Any())
        {
            return Json(
                Array.Empty<string>());
        }

        var wizyty =
            await _context.Wizyty
                .Where(x =>
                    x.LekarzId == lekarzId &&
                    x.DataWizyty.Date ==
                    data.Date &&
                    x.Status !=
                    StatusWizyty.Anulowana)
                .ToListAsync();

        var dostepneGodziny =
            new List<string>();

        foreach (var grafik in grafiki)
        {
            var aktualnaGodzina =
                grafik.GodzinaOd;

            while (
                aktualnaGodzina.Add(
                    TimeSpan.FromMinutes(30))
                <= grafik.GodzinaDo)
            {
                var poczatekSlotu =
                    data.Date.Add(
                        aktualnaGodzina);

                var koniecSlotu =
                    poczatekSlotu.AddMinutes(30);

                if (poczatekSlotu > DateTime.Now)
                {
                    var zajety =
                        wizyty.Any(wizyta =>
                        {
                            var poczatekWizyty =
                                wizyta.DataWizyty;

                            var koniecWizyty =
                                wizyta.DataWizyty
                                    .AddMinutes(30);

                            return
                                poczatekWizyty <
                                koniecSlotu &&
                                koniecWizyty >
                                poczatekSlotu;
                        });

                    if (!zajety)
                    {
                        dostepneGodziny.Add(
                            aktualnaGodzina
                                .ToString(
                                    @"hh\:mm"));
                    }
                }

                aktualnaGodzina =
                    aktualnaGodzina.Add(
                        TimeSpan.FromMinutes(30));
            }
        }

        return Json(
            dostepneGodziny
                .Distinct()
                .OrderBy(x => x));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Umow(
        UmowWizyteViewModel model)
    {
        var pacjent =
            await PobierzLubUtworzPacjenta();

        if (pacjent == null)
            return NotFound();

        var lekarzIstnieje =
            await _context.Lekarze.AnyAsync(x =>
                x.Id == model.LekarzId);

        if (!lekarzIstnieje)
        {
            ModelState.AddModelError(
                "LekarzId",
                "Wybrany lekarz nie istnieje.");
        }

        if (model.Data.Date <
            DateTime.Today)
        {
            ModelState.AddModelError(
                "Data",
                "Nie można wybrać daty z przeszłości.");
        }

        if (!TimeSpan.TryParse(
            model.Godzina,
            out var wybranaGodzina))
        {
            ModelState.AddModelError(
                "Godzina",
                "Wybierz poprawną godzinę.");
        }

        if (ModelState.IsValid)
        {
            if (wybranaGodzina.Minutes != 0 &&
                wybranaGodzina.Minutes != 30)
            {
                ModelState.AddModelError(
                    "Godzina",
                    "Wizyty można umawiać co 30 minut.");
            }

            if (wybranaGodzina.Seconds != 0)
            {
                ModelState.AddModelError(
                    "Godzina",
                    "Nieprawidłowa godzina wizyty.");
            }
        }

        if (!ModelState.IsValid)
        {
            PrzygotujLekarzy();

            return View(model);
        }

        var dataWizyty =
            model.Data.Date.Add(
                wybranaGodzina);

        if (dataWizyty <= DateTime.Now)
        {
            ModelState.AddModelError(
                "",
                "Nie można umówić wizyty w przeszłości.");
        }

        var koniecWizyty =
            dataWizyty.AddMinutes(30);

        var dostepnyGrafik =
            await _context.GrafikiLekarzy
                .AnyAsync(x =>
                    x.LekarzId ==
                    model.LekarzId &&
                    x.Data.Date ==
                    model.Data.Date &&
                    x.CzyDostepny &&
                    wybranaGodzina >=
                    x.GodzinaOd &&
                    wybranaGodzina.Add(
                        TimeSpan.FromMinutes(30))
                    <= x.GodzinaDo);

        if (!dostepnyGrafik)
        {
            ModelState.AddModelError(
                "",
                "Lekarz nie jest dostępny w wybranym terminie.");
        }

        var zajetyTermin =
            await _context.Wizyty
                .AnyAsync(x =>
                    x.LekarzId ==
                    model.LekarzId &&
                    x.Status !=
                    StatusWizyty.Anulowana &&
                    x.DataWizyty <
                    koniecWizyty &&
                    x.DataWizyty
                        .AddMinutes(30) >
                    dataWizyty);

        if (zajetyTermin)
        {
            ModelState.AddModelError(
                "",
                "Wybrany termin został już zajęty.");
        }

        if (!ModelState.IsValid)
        {
            PrzygotujLekarzy();

            return View(model);
        }

        var wizyta =
            new Wizyta
            {
                PacjentId =
                    pacjent.Id,

                LekarzId =
                    model.LekarzId,

                DataWizyty =
                    dataWizyty,

                Uwagi =
                    model.Uwagi,

                Status =
                    StatusWizyty.Zaplanowana
            };

        _context.Wizyty.Add(
            wizyta);

        await _context.SaveChangesAsync();

        TempData["Sukces"] =
            "Wizyta została pomyślnie umówiona.";

        return RedirectToAction(
            nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anuluj(
        int id)
    {
        var pacjent =
            await PobierzLubUtworzPacjenta();

        if (pacjent == null)
            return NotFound();

        var wizyta =
            await _context.Wizyty
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.PacjentId ==
                    pacjent.Id);

        if (wizyta == null)
            return NotFound();

        if (wizyta.Status !=
            StatusWizyty.Zaplanowana)
        {
            TempData["Blad"] =
                "Tej wizyty nie można anulować.";

            return RedirectToAction(
                nameof(Index));
        }

        if (wizyta.DataWizyty <=
            DateTime.Now)
        {
            TempData["Blad"] =
                "Nie można anulować wizyty, której termin już minął.";

            return RedirectToAction(
                nameof(Index));
        }

        wizyta.Status =
            StatusWizyty.Anulowana;

        await _context.SaveChangesAsync();

        TempData["Sukces"] =
            "Wizyta została anulowana.";

        return RedirectToAction(
            nameof(Index));
    }

    private void PrzygotujLekarzy()
    {
        var lekarze =
            _context.Lekarze
                .Include(x =>
                    x.Specjalizacja)
                .Include(x =>
                    x.PlacowkaMedyczna)
                .OrderBy(x =>
                    x.Nazwisko)
                .ThenBy(x =>
                    x.Imie)
                .ToList()
                .Select(x => new
                {
                    x.Id,

                    Nazwa =
                        x.Imie + " " +
                        x.Nazwisko + " - " +
                        x.Specjalizacja?.Nazwa +
                        " - " +
                        x.PlacowkaMedyczna?.Nazwa
                });

        ViewBag.Lekarze =
            new SelectList(
                lekarze,
                "Id",
                "Nazwa");
    }

    private async Task<Pacjent?>
        PobierzLubUtworzPacjenta()
    {
        var uzytkownik =
            await _userManager
                .GetUserAsync(User);

        if (uzytkownik == null)
            return null;

        var pacjent =
            await _context.Pacjenci
                .FirstOrDefaultAsync(x =>
                    x.UzytkownikId ==
                    uzytkownik.Id);

        if (pacjent != null)
            return pacjent;

        pacjent =
            new Pacjent
            {
                Imie =
                    uzytkownik.Imie,

                Nazwisko =
                    uzytkownik.Nazwisko,

                Email =
                    uzytkownik.Email ??
                    string.Empty,

                UzytkownikId =
                    uzytkownik.Id
            };

        _context.Pacjenci.Add(
            pacjent);

        await _context.SaveChangesAsync();

        return pacjent;
    }
}