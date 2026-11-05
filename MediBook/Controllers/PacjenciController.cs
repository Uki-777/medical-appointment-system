using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

public class PacjenciController : Controller
{
    private readonly ApplicationDbContext _context;

    public PacjenciController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Pacjenci.ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pacjent pacjent)
    {
        if (!ModelState.IsValid)
            return View(pacjent);

        _context.Add(pacjent);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var pacjent =
            await _context.Pacjenci.FindAsync(id);

        if (pacjent == null)
            return NotFound();

        return View(pacjent);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Pacjent pacjent)
    {
        if (id != pacjent.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(pacjent);

        _context.Update(pacjent);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var pacjent =
            await _context.Pacjenci
                .FirstOrDefaultAsync(x => x.Id == id);

        if (pacjent == null)
            return NotFound();

        return View(pacjent);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var pacjent =
            await _context.Pacjenci.FindAsync(id);

        if (pacjent != null)
        {
            _context.Pacjenci.Remove(pacjent);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}