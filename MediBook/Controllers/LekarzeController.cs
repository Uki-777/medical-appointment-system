using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

public class LekarzeController : Controller
{
    private readonly ApplicationDbContext _context;

    public LekarzeController(ApplicationDbContext context)
    {
        _context = context;
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
    public async Task<IActionResult> Create(Lekarz lekarz)
    {
        if (!ModelState.IsValid)
        {
            PrzygotujListy();
            return View(lekarz);
        }

        _context.Add(lekarz);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var lekarz = await _context.Lekarze.FindAsync(id);

        if (lekarz == null)
            return NotFound();

        PrzygotujListy();

        return View(lekarz);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Lekarz lekarz)
    {
        if (id != lekarz.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            PrzygotujListy();
            return View(lekarz);
        }

        _context.Update(lekarz);
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
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var lekarz = await _context.Lekarze.FindAsync(id);

        if (lekarz != null)
        {
            _context.Lekarze.Remove(lekarz);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private void PrzygotujListy()
    {
        ViewBag.Specjalizacje = new SelectList(
            _context.Specjalizacje,
            "Id",
            "Nazwa");

        ViewBag.Placowki = new SelectList(
            _context.PlacowkiMedyczne,
            "Id",
            "Nazwa");
    }
}