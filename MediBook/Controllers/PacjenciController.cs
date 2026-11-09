using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

[Authorize(Roles = "Administrator")]
public class PacjenciController : Controller
{
    private readonly ApplicationDbContext _context;

    public PacjenciController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var pacjenci =
            await _context.Pacjenci
                .OrderBy(x => x.Nazwisko)
                .ThenBy(x => x.Imie)
                .ToListAsync();

        return View(pacjenci);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Pacjent pacjent)
    {
        var istniejeEmail =
            await _context.Pacjenci.AnyAsync(x =>
                x.Email == pacjent.Email);

        if (istniejeEmail)
        {
            ModelState.AddModelError(
                "Email",
                "Pacjent z takim adresem email już istnieje.");
        }

        if (pacjent.DataUrodzenia.HasValue &&
            pacjent.DataUrodzenia.Value.Date >
            DateTime.Today)
        {
            ModelState.AddModelError(
                "DataUrodzenia",
                "Data urodzenia nie może być datą przyszłą.");
        }

        if (!ModelState.IsValid)
        {
            return View(pacjent);
        }

        _context.Pacjenci.Add(pacjent);

        await _context.SaveChangesAsync();

        TempData["Sukces"] =
            "Pacjent został dodany.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(
        int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var pacjent =
            await _context.Pacjenci
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (pacjent == null)
        {
            return NotFound();
        }

        return View(pacjent);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Pacjent pacjent)
    {
        if (id != pacjent.Id)
        {
            return NotFound();
        }

        var pacjentIstnieje =
            await _context.Pacjenci.AnyAsync(x =>
                x.Id == id);

        if (!pacjentIstnieje)
        {
            return NotFound();
        }

        var istniejeEmail =
            await _context.Pacjenci.AnyAsync(x =>
                x.Email == pacjent.Email &&
                x.Id != id);

        if (istniejeEmail)
        {
            ModelState.AddModelError(
                "Email",
                "Pacjent z takim adresem email już istnieje.");
        }

        if (pacjent.DataUrodzenia.HasValue &&
            pacjent.DataUrodzenia.Value.Date >
            DateTime.Today)
        {
            ModelState.AddModelError(
                "DataUrodzenia",
                "Data urodzenia nie może być datą przyszłą.");
        }

        if (!ModelState.IsValid)
        {
            return View(pacjent);
        }

        await _context.Pacjenci
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(settery =>
                settery
                    .SetProperty(
                        x => x.Imie,
                        pacjent.Imie)
                    .SetProperty(
                        x => x.Nazwisko,
                        pacjent.Nazwisko)
                    .SetProperty(
                        x => x.Email,
                        pacjent.Email)
                    .SetProperty(
                        x => x.NumerTelefonu,
                        pacjent.NumerTelefonu)
                    .SetProperty(
                        x => x.DataUrodzenia,
                        pacjent.DataUrodzenia));

        TempData["Sukces"] =
            "Dane pacjenta zostały zaktualizowane.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(
        int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var pacjent =
            await _context.Pacjenci
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (pacjent == null)
        {
            return NotFound();
        }

        return View(pacjent);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        var pacjent =
            await _context.Pacjenci
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (pacjent == null)
        {
            return NotFound();
        }

        var posiadaWizyty =
            await _context.Wizyty.AnyAsync(x =>
                x.PacjentId == id);

        if (posiadaWizyty)
        {
            TempData["Blad"] =
                "Nie można usunąć pacjenta, który posiada historię wizyt.";

            return RedirectToAction(nameof(Index));
        }

        _context.Pacjenci.Remove(pacjent);

        await _context.SaveChangesAsync();

        TempData["Sukces"] =
            "Pacjent został usunięty.";

        return RedirectToAction(nameof(Index));
    }
}