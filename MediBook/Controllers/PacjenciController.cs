using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

[Authorize(Roles = "Administrator")]
public class PacjenciController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Uzytkownik> _userManager;

    public PacjenciController(
        ApplicationDbContext context,
        UserManager<Uzytkownik> userManager)
    {
        _context = context;
        _userManager = userManager;
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

        var aktualnyPacjent =
            await _context.Pacjenci
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (aktualnyPacjent == null)
        {
            return NotFound();
        }

        pacjent.Email =
            pacjent.Email
                .Trim()
                .ToLower();

        var istniejeEmailPacjenta =
            await _context.Pacjenci
                .AnyAsync(x =>
                    x.Id != id &&
                    x.Email.ToLower() ==
                    pacjent.Email);

        if (istniejeEmailPacjenta)
        {
            ModelState.AddModelError(
                "Email",
                "Pacjent z takim adresem email już istnieje.");
        }

        var istniejeKonto =
            await _userManager
                .FindByEmailAsync(
                    pacjent.Email);

        if (istniejeKonto != null &&
            istniejeKonto.Id !=
            aktualnyPacjent.UzytkownikId)
        {
            ModelState.AddModelError(
                "Email",
                "Konto z takim adresem email już istnieje.");
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

        if (!string.IsNullOrWhiteSpace(
            aktualnyPacjent.UzytkownikId))
        {
            var uzytkownik =
                await _userManager.FindByIdAsync(
                    aktualnyPacjent.UzytkownikId);

            if (uzytkownik != null)
            {
                uzytkownik.Imie =
                    pacjent.Imie;

                uzytkownik.Nazwisko =
                    pacjent.Nazwisko;

                uzytkownik.Email =
                    pacjent.Email;

                uzytkownik.UserName =
                    pacjent.Email;

                var wynik =
                    await _userManager
                        .UpdateAsync(
                            uzytkownik);

                if (!wynik.Succeeded)
                {
                    foreach (var blad
                             in wynik.Errors)
                    {
                        ModelState.AddModelError(
                            "",
                            blad.Description);
                    }

                    return View(pacjent);
                }
            }
        }

        await _context.Pacjenci
            .Where(x =>
                x.Id == id)
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

        return RedirectToAction(
            nameof(Index));
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
            await _context.Wizyty
                .AnyAsync(x =>
                    x.PacjentId == id);

        if (posiadaWizyty)
        {
            TempData["Blad"] =
                "Nie można usunąć pacjenta, który posiada historię wizyt.";

            return RedirectToAction(
                nameof(Index));
        }

        Uzytkownik? uzytkownik = null;

        if (!string.IsNullOrWhiteSpace(
            pacjent.UzytkownikId))
        {
            uzytkownik =
                await _userManager
                    .FindByIdAsync(
                        pacjent.UzytkownikId);
        }

        _context.Pacjenci.Remove(
            pacjent);

        await _context.SaveChangesAsync();

        if (uzytkownik != null)
        {
            await _userManager
                .DeleteAsync(
                    uzytkownik);
        }

        TempData["Sukces"] =
            "Pacjent został usunięty.";

        return RedirectToAction(
            nameof(Index));
    }
}