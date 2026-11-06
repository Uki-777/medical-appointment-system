using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

[Authorize(Roles = "Lekarz")]
public class PanelLekarzaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Uzytkownik> _userManager;

    public PanelLekarzaController(
        ApplicationDbContext context,
        UserManager<Uzytkownik> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var lekarz = await PobierzLekarza();

        if (lekarz == null)
            return NotFound();

        var wizyty = await _context.Wizyty
            .Where(x => x.LekarzId == lekarz.Id)
            .Include(x => x.Pacjent)
            .OrderBy(x => x.DataWizyty)
            .ToListAsync();

        return View(wizyty);
    }

    public async Task<IActionResult> Grafik()
    {
        var lekarz = await PobierzLekarza();

        if (lekarz == null)
            return NotFound();

        var grafik = await _context.GrafikiLekarzy
            .Where(x => x.LekarzId == lekarz.Id)
            .OrderBy(x => x.Data)
            .ThenBy(x => x.GodzinaOd)
            .ToListAsync();

        return View(grafik);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Zakoncz(int id)
    {
        var lekarz = await PobierzLekarza();

        if (lekarz == null)
            return NotFound();

        var wizyta =
            await _context.Wizyty
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.LekarzId == lekarz.Id);

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nieobecnosc(int id)
    {
        var lekarz = await PobierzLekarza();

        if (lekarz == null)
            return NotFound();

        var wizyta =
            await _context.Wizyty
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.LekarzId == lekarz.Id);

        if (wizyta == null)
            return NotFound();

        if (wizyta.Status ==
            StatusWizyty.Zaplanowana)
        {
            wizyta.Status =
                StatusWizyty.Nieobecnosc;

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Lekarz?> PobierzLekarza()
    {
        var uzytkownik =
            await _userManager.GetUserAsync(User);

        if (uzytkownik == null)
            return null;

        return await _context.Lekarze
            .Include(x => x.Specjalizacja)
            .Include(x => x.PlacowkaMedyczna)
            .FirstOrDefaultAsync(x =>
                x.UzytkownikId == uzytkownik.Id);
    }
}