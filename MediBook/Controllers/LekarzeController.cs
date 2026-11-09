using MediBook.Data;
using MediBook.Models;
using MediBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

[Authorize(Roles = "Administrator")]
public class LekarzeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Uzytkownik> _userManager;

    public LekarzeController(
        ApplicationDbContext context,
        UserManager<Uzytkownik> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var lekarze =
            await _context.Lekarze
                .Include(x => x.Specjalizacja)
                .Include(x => x.PlacowkaMedyczna)
                .OrderBy(x => x.Nazwisko)
                .ThenBy(x => x.Imie)
                .ToListAsync();

        return View(lekarze);
    }

    public IActionResult Create()
    {
        PrzygotujListy();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        DodajLekarzaViewModel model)
    {
        var specjalizacjaIstnieje =
            await _context.Specjalizacje.AnyAsync(x =>
                x.Id == model.SpecjalizacjaId);

        if (!specjalizacjaIstnieje)
        {
            ModelState.AddModelError(
                "SpecjalizacjaId",
                "Wybrana specjalizacja nie istnieje.");
        }

        var placowkaIstnieje =
            await _context.PlacowkiMedyczne.AnyAsync(x =>
                x.Id == model.PlacowkaMedycznaId);

        if (!placowkaIstnieje)
        {
            ModelState.AddModelError(
                "PlacowkaMedycznaId",
                "Wybrana placówka nie istnieje.");
        }

        var istniejeUzytkownik =
            await _userManager.FindByEmailAsync(
                model.Email);

        if (istniejeUzytkownik != null)
        {
            ModelState.AddModelError(
                "Email",
                "Konto z takim adresem email już istnieje.");
        }

        var istniejeLekarz =
            await _context.Lekarze.AnyAsync(x =>
                x.Email == model.Email);

        if (istniejeLekarz)
        {
            ModelState.AddModelError(
                "Email",
                "Lekarz z takim adresem email już istnieje.");
        }

        if (!ModelState.IsValid)
        {
            PrzygotujListy();

            return View(model);
        }

        var uzytkownik = new Uzytkownik
        {
            UserName = model.Email,
            Email = model.Email,
            Imie = model.Imie,
            Nazwisko = model.Nazwisko
        };

        var wynik =
            await _userManager.CreateAsync(
                uzytkownik,
                model.HasloStartowe);

        if (!wynik.Succeeded)
        {
            foreach (var blad in wynik.Errors)
            {
                ModelState.AddModelError(
                    "",
                    blad.Description);
            }

            PrzygotujListy();

            return View(model);
        }

        var wynikRoli =
            await _userManager.AddToRoleAsync(
                uzytkownik,
                "Lekarz");

        if (!wynikRoli.Succeeded)
        {
            await _userManager.DeleteAsync(
                uzytkownik);

            ModelState.AddModelError(
                "",
                "Nie udało się przypisać roli lekarza.");

            PrzygotujListy();

            return View(model);
        }

        var lekarz = new Lekarz
        {
            Imie = model.Imie,
            Nazwisko = model.Nazwisko,
            Email = model.Email,
            UzytkownikId = uzytkownik.Id,
            SpecjalizacjaId =
                model.SpecjalizacjaId,
            PlacowkaMedycznaId =
                model.PlacowkaMedycznaId
        };

        _context.Lekarze.Add(lekarz);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var lekarz =
            await _context.Lekarze.FindAsync(id);

        if (lekarz == null)
            return NotFound();

        PrzygotujListy();

        return View(lekarz);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Lekarz lekarz)
    {
        if (id != lekarz.Id)
            return NotFound();

        var specjalizacjaIstnieje =
            await _context.Specjalizacje.AnyAsync(x =>
                x.Id == lekarz.SpecjalizacjaId);

        if (!specjalizacjaIstnieje)
        {
            ModelState.AddModelError(
                "SpecjalizacjaId",
                "Wybrana specjalizacja nie istnieje.");
        }

        var placowkaIstnieje =
            await _context.PlacowkiMedyczne.AnyAsync(x =>
                x.Id == lekarz.PlacowkaMedycznaId);

        if (!placowkaIstnieje)
        {
            ModelState.AddModelError(
                "PlacowkaMedycznaId",
                "Wybrana placówka nie istnieje.");
        }

        if (!ModelState.IsValid)
        {
            PrzygotujListy();

            return View(lekarz);
        }

        var aktualny =
            await _context.Lekarze.FindAsync(id);

        if (aktualny == null)
            return NotFound();

        aktualny.Imie =
            lekarz.Imie;

        aktualny.Nazwisko =
            lekarz.Nazwisko;

        aktualny.SpecjalizacjaId =
            lekarz.SpecjalizacjaId;

        aktualny.PlacowkaMedycznaId =
            lekarz.PlacowkaMedycznaId;

        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(
            aktualny.UzytkownikId))
        {
            var uzytkownik =
                await _userManager.FindByIdAsync(
                    aktualny.UzytkownikId);

            if (uzytkownik != null)
            {
                uzytkownik.Imie =
                    aktualny.Imie;

                uzytkownik.Nazwisko =
                    aktualny.Nazwisko;

                await _userManager.UpdateAsync(
                    uzytkownik);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var lekarz =
            await _context.Lekarze
                .Include(x => x.Specjalizacja)
                .Include(x => x.PlacowkaMedyczna)
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (lekarz == null)
            return NotFound();

        return View(lekarz);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        var lekarz =
            await _context.Lekarze.FindAsync(id);

        if (lekarz == null)
            return NotFound();

        var posiadaWizyty =
            await _context.Wizyty.AnyAsync(x =>
                x.LekarzId == id);

        var posiadaGrafiki =
            await _context.GrafikiLekarzy.AnyAsync(x =>
                x.LekarzId == id);

        if (posiadaWizyty ||
            posiadaGrafiki)
        {
            TempData["Blad"] =
                "Nie można usunąć lekarza posiadającego wizyty lub grafik.";

            return RedirectToAction(nameof(Index));
        }

        Uzytkownik? uzytkownik = null;

        if (!string.IsNullOrWhiteSpace(
            lekarz.UzytkownikId))
        {
            uzytkownik =
                await _userManager.FindByIdAsync(
                    lekarz.UzytkownikId);
        }

        _context.Lekarze.Remove(lekarz);

        await _context.SaveChangesAsync();

        if (uzytkownik != null)
        {
            await _userManager.DeleteAsync(
                uzytkownik);
        }

        return RedirectToAction(nameof(Index));
    }

    private void PrzygotujListy()
    {
        ViewBag.Specjalizacje =
            new SelectList(
                _context.Specjalizacje
                    .OrderBy(x => x.Nazwa),
                "Id",
                "Nazwa");

        ViewBag.Placowki =
            new SelectList(
                _context.PlacowkiMedyczne
                    .OrderBy(x => x.Nazwa),
                "Id",
                "Nazwa");
    }
}