using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

[Authorize(Roles = "Administrator")]
public class GrafikiLekarzyController : Controller
{
    private readonly ApplicationDbContext _context;

    public GrafikiLekarzyController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var grafiki = await _context.GrafikiLekarzy
            .Include(x => x.Lekarz)
            .OrderBy(x => x.Data)
            .ThenBy(x => x.GodzinaOd)
            .ToListAsync();

        return View(grafiki);
    }

    public IActionResult Create()
    {
        PrzygotujLekarzy();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GrafikLekarza grafik)
    {
        var lekarzIstnieje =
            await _context.Lekarze.AnyAsync(x =>
                x.Id == grafik.LekarzId);

        if (!lekarzIstnieje)
        {
            ModelState.AddModelError(
                "LekarzId",
                "Wybrany lekarz nie istnieje.");
        }

        if (grafik.Data.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                "Data",
                "Nie można utworzyć grafiku w przeszłości.");
        }

        if (grafik.GodzinaDo <= grafik.GodzinaOd)
        {
            ModelState.AddModelError(
                "",
                "Godzina zakończenia musi być późniejsza niż godzina rozpoczęcia.");
        }

        if (grafik.GodzinaOd.Minutes % 30 != 0 ||
            grafik.GodzinaDo.Minutes % 30 != 0 ||
            grafik.GodzinaOd.Seconds != 0 ||
            grafik.GodzinaDo.Seconds != 0)
        {
            ModelState.AddModelError(
                "",
                "Grafik może zaczynać i kończyć się tylko o pełnej godzinie lub 30 minut po godzinie.");
        }

        var nakladajacySieGrafik =
            await _context.GrafikiLekarzy.AnyAsync(x =>
                x.LekarzId == grafik.LekarzId &&
                x.Data.Date == grafik.Data.Date &&
                x.GodzinaOd < grafik.GodzinaDo &&
                x.GodzinaDo > grafik.GodzinaOd);

        if (nakladajacySieGrafik)
        {
            ModelState.AddModelError(
                "",
                "Lekarz ma już grafik nachodzący na wybrane godziny.");
        }

        if (!ModelState.IsValid)
        {
            PrzygotujLekarzy();

            return View(grafik);
        }

        grafik.CzyDostepny = true;
        
        _context.GrafikiLekarzy.Add(grafik);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var grafik =
            await _context.GrafikiLekarzy.FindAsync(id);

        if (grafik == null)
            return NotFound();

        var istniejaWizyty =
            await _context.Wizyty.AnyAsync(x =>
                x.LekarzId == grafik.LekarzId &&
                x.DataWizyty.Date == grafik.Data.Date &&
                x.DataWizyty.TimeOfDay >= grafik.GodzinaOd &&
                x.DataWizyty.TimeOfDay < grafik.GodzinaDo &&
                x.Status == StatusWizyty.Zaplanowana);

        if (istniejaWizyty)
        {
            TempData["Blad"] =
                "Nie można usunąć grafiku, ponieważ istnieją w nim zaplanowane wizyty.";

            return RedirectToAction(nameof(Index));
        }

        _context.GrafikiLekarzy.Remove(grafik);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private void PrzygotujLekarzy()
    {
        ViewBag.Lekarze =
            new SelectList(
                _context.Lekarze
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
    }
}