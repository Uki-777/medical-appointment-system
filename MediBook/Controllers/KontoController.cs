using MediBook.Data;
using MediBook.Models;
using MediBook.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

public class KontoController : Controller
{
    private readonly UserManager<Uzytkownik> _userManager;
    private readonly SignInManager<Uzytkownik> _signInManager;
    private readonly ApplicationDbContext _context;

    public KontoController(
        UserManager<Uzytkownik> userManager,
        SignInManager<Uzytkownik> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public IActionResult Rejestracja()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rejestracja(
        RejestracjaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var uzytkownik = new Uzytkownik
        {
            UserName = model.Email,
            Email = model.Email,
            Imie = model.Imie,
            Nazwisko = model.Nazwisko
        };

        var wynik = await _userManager.CreateAsync(
            uzytkownik,
            model.Haslo);

        if (wynik.Succeeded)
        {
            await _userManager.AddToRoleAsync(
                uzytkownik,
                "Pacjent");

            var pacjent = new Pacjent
            {
                Imie = model.Imie,
                Nazwisko = model.Nazwisko,
                Email = model.Email,
                UzytkownikId = uzytkownik.Id
            };

            _context.Pacjenci.Add(pacjent);

            await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(
                uzytkownik,
                false);

            return RedirectToAction(
                "Index",
                "Home");
        }

        foreach (var blad in wynik.Errors)
        {
            ModelState.AddModelError(
                "",
                blad.Description);
        }

        return View(model);
    }

    public IActionResult Logowanie()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logowanie(
        LogowanieViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var wynik =
            await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Haslo,
                model.ZapamietajMnie,
                false);

        if (wynik.Succeeded)
        {
            return RedirectToAction(
                "Index",
                "Home");
        }

        ModelState.AddModelError(
            "",
            "Nieprawidłowy email lub hasło.");

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Wyloguj()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction(
            "Index",
            "Home");
    }

    public IActionResult BrakDostepu()
    {
        return View();
    }
}