using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

public class PlacowkiMedyczneController : Controller
{
    private readonly ApplicationDbContext _context;

    public PlacowkiMedyczneController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.PlacowkiMedyczne.ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlacowkaMedyczna placowka)
    {
        if (!ModelState.IsValid)
            return View(placowka);

        _context.Add(placowka);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var placowka = await _context.PlacowkiMedyczne.FindAsync(id);

        if (placowka == null)
            return NotFound();

        return View(placowka);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PlacowkaMedyczna placowka)
    {
        if (id != placowka.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(placowka);

        _context.Update(placowka);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var placowka = await _context.PlacowkiMedyczne
            .FirstOrDefaultAsync(x => x.Id == id);

        if (placowka == null)
            return NotFound();

        return View(placowka);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var placowka = await _context.PlacowkiMedyczne.FindAsync(id);

        if (placowka != null)
        {
            _context.PlacowkiMedyczne.Remove(placowka);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}