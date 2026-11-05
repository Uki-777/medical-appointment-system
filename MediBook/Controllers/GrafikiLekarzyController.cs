using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

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
        ViewBag.Lekarze = new SelectList(
            _context.Lekarze
                .Select(x => new
                {
                    x.Id,
                    Nazwa = x.Imie + " " + x.Nazwisko
                }),
            "Id",
            "Nazwa");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GrafikLekarza grafik)
    {
        if (grafik.GodzinaDo <= grafik.GodzinaOd)
        {
            ModelState.AddModelError(
                "",
                "Godzina zakończenia musi być późniejsza niż rozpoczęcia.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Lekarze = new SelectList(
                _context.Lekarze.Select(x => new
                {
                    x.Id,
                    Nazwa = x.Imie + " " + x.Nazwisko
                }),
                "Id",
                "Nazwa");

            return View(grafik);
        }

        _context.Add(grafik);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var grafik =
            await _context.GrafikiLekarzy.FindAsync(id);

        if (grafik != null)
        {
            _context.GrafikiLekarzy.Remove(grafik);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}