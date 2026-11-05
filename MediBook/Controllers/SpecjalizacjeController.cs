using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

public class SpecjalizacjeController : Controller
{
    private readonly ApplicationDbContext _context;

    public SpecjalizacjeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Specjalizacje.ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Specjalizacja specjalizacja)
    {
        if (ModelState.IsValid)
        {
            _context.Add(specjalizacja);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(specjalizacja);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var specjalizacja = await _context.Specjalizacje.FindAsync(id);

        if (specjalizacja == null)
        {
            return NotFound();
        }

        return View(specjalizacja);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Specjalizacja specjalizacja)
    {
        if (id != specjalizacja.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(specjalizacja);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(specjalizacja);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var specjalizacja = await _context.Specjalizacje
            .FirstOrDefaultAsync(m => m.Id == id);

        if (specjalizacja == null)
        {
            return NotFound();
        }

        return View(specjalizacja);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var specjalizacja = await _context.Specjalizacje.FindAsync(id);

        if (specjalizacja != null)
        {
            _context.Specjalizacje.Remove(specjalizacja);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}