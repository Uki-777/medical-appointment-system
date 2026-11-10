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
        if (User.Identity != null &&
            User.Identity.IsAuthenticated)
        {
            return RedirectToAction(
                "Index",
                "Home");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rejestracja(
        RejestracjaViewModel model)
    {
        if (User.Identity != null &&
            User.Identity.IsAuthenticated)
        {
            return RedirectToAction(
                "Index",
                "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Email =
            model.Email
                .Trim()
                .ToLower();

        var istniejeUzytkownik =
            await _userManager
                .FindByEmailAsync(
                    model.Email);

        if (istniejeUzytkownik != null)
        {
            ModelState.AddModelError(
                "Email",
                "Konto z takim adresem email już istnieje.");

            return View(model);
        }

        var istniejePacjent =
            await _context.Pacjenci
                .AnyAsync(x =>
                    x.Email.ToLower() ==
                    model.Email);

        if (istniejePacjent)
        {
            ModelState.AddModelError(
                "Email",
                "Pacjent z takim adresem email już istnieje.");

            return View(model);
        }

        var uzytkownik =
            new Uzytkownik
            {
                UserName =
                    model.Email,

                Email =
                    model.Email,

                Imie =
                    model.Imie,

                Nazwisko =
                    model.Nazwisko
            };

        var wynik =
            await _userManager
                .CreateAsync(
                    uzytkownik,
                    model.Haslo);

        if (!wynik.Succeeded)
        {
            foreach (var blad
                     in wynik.Errors)
            {
                ModelState.AddModelError(
                    "",
                    blad.Description);
            }

            return View(model);
        }

        var wynikRoli =
            await _userManager
                .AddToRoleAsync(
                    uzytkownik,
                    "Pacjent");

        if (!wynikRoli.Succeeded)
        {
            await _userManager
                .DeleteAsync(
                    uzytkownik);

            ModelState.AddModelError(
                "",
                "Nie udało się utworzyć konta pacjenta.");

            return View(model);
        }

        var pacjent =
            new Pacjent
            {
                Imie =
                    model.Imie,

                Nazwisko =
                    model.Nazwisko,

                Email =
                    model.Email,

                UzytkownikId =
                    uzytkownik.Id
            };

        _context.Pacjenci.Add(
            pacjent);

        try
        {
            await _context
                .SaveChangesAsync();
        }
        catch
        {
            await _userManager
                .DeleteAsync(
                    uzytkownik);

            ModelState.AddModelError(
                "",
                "Nie udało się zakończyć rejestracji.");

            return View(model);
        }

        await _signInManager
            .SignInAsync(
                uzytkownik,
                false);

        TempData["Sukces"] =
            "Konto zostało utworzone.";

        return RedirectToAction(
            "Index",
            "Home");
    }

    public IActionResult Logowanie()
    {
        if (User.Identity != null &&
            User.Identity.IsAuthenticated)
        {
            return RedirectToAction(
                "Index",
                "Home");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logowanie(
        LogowanieViewModel model)
    {
        if (User.Identity != null &&
            User.Identity.IsAuthenticated)
        {
            return RedirectToAction(
                "Index",
                "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Email =
            model.Email
                .Trim()
                .ToLower();

        var wynik =
            await _signInManager
                .PasswordSignInAsync(
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
        await _signInManager
            .SignOutAsync();

        return RedirectToAction(
            "Index",
            "Home");
    }

    public IActionResult BrakDostepu()
    {
        return View();
    }
}