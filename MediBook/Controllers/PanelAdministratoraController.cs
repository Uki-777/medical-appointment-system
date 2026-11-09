using MediBook.Data;
using MediBook.Models;
using MediBook.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers;

[Authorize(Roles = "Administrator")]
public class PanelAdministratoraController : Controller
{
    private readonly ApplicationDbContext _context;

    public PanelAdministratoraController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var dzisiaj = DateTime.Today;
        var jutro = dzisiaj.AddDays(1);

        var model = new PanelAdministratoraViewModel
        {
            LiczbaPacjentow =
                await _context.Pacjenci.CountAsync(),

            LiczbaLekarzy =
                await _context.Lekarze.CountAsync(),

            LiczbaPlacowek =
                await _context.PlacowkiMedyczne.CountAsync(),

            LiczbaWizyt =
                await _context.Wizyty.CountAsync(),

            LiczbaDzisiejszychWizyt =
                await _context.Wizyty.CountAsync(x =>
                    x.DataWizyty >= dzisiaj &&
                    x.DataWizyty < jutro &&
                    x.Status != StatusWizyty.Anulowana),

            LiczbaNadchodzacychWizyt =
                await _context.Wizyty.CountAsync(x =>
                    x.DataWizyty > DateTime.Now &&
                    x.Status == StatusWizyty.Zaplanowana)
        };

        return View(model);
    }
}