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
        var lekarze = await _context.Lekarze
            .Include(x => x.Specjalizacja)
            .Include(x => x.PlacowkaMedyczna)
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
        if (!ModelState.IsValid)
        {
            PrzygotujListy();
            return View(model);
        }

        var istnieje =
            await _userManager.FindByEmailAsync(model.Email);

        if (istnieje != null)
        {
            ModelState.AddModelError(
                "Email",
                "Konto z takim adresem email już istnieje.");

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

        await _userManager.AddToRoleAsync(
            uzytkownik,
            "Lekarz");

        var lekarz = new Lekarz
        {
            Imie = model.Imie,
            Nazwisko = model.Nazwisko,
            Email = model.Email,
            UzytkownikId = uzytkownik.Id,
            SpecjalizacjaId = model.SpecjalizacjaId,
            PlacowkaMedycznaId = model.PlacowkaMedycznaId
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

        if (!ModelState.IsValid)
        {
            PrzygotujListy();

            return View(lekarz);
        }

        var aktualny =
            await _context.Lekarze.FindAsync(id);

        if (aktualny == null)
            return NotFound();

        aktualny.Imie = lekarz.Imie;
        aktualny.Nazwisko = lekarz.Nazwisko;
        aktualny.SpecjalizacjaId =
            lekarz.SpecjalizacjaId;
        aktualny.PlacowkaMedycznaId =
            lekarz.PlacowkaMedycznaId;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var lekarz = await _context.Lekarze
            .Include(x => x.Specjalizacja)
            .Include(x => x.PlacowkaMedyczna)
            .FirstOrDefaultAsync(x => x.Id == id);

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

        if (lekarz != null)
        {
            _context.Lekarze.Remove(lekarz);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private void PrzygotujListy()
    {
        ViewBag.Specjalizacje =
            new SelectList(
                _context.Specjalizacje,
                "Id",
                "Nazwa");

        ViewBag.Placowki =
            new SelectList(
                _context.PlacowkiMedyczne,
                "Id",
                "Nazwa");
    }
}