using MediBook.Data;
using MediBook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<Uzytkownik, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Konto/Logowanie";
    options.AccessDeniedPath = "/Konto/BrakDostepu";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await InicjalizujRoleIAdministratora(app);

app.Run();

static async Task InicjalizujRoleIAdministratora(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<Uzytkownik>>();

    string[] role =
    {
        "Administrator",
        "Lekarz",
        "Pacjent"
    };

    foreach (var nazwa in role)
    {
        if (!await roleManager.RoleExistsAsync(nazwa))
        {
            await roleManager.CreateAsync(
                new IdentityRole(nazwa));
        }
    }

    var hasloAdministratora =
        Environment.GetEnvironmentVariable(
            "MEDIBOOK_ADMIN_PASSWORD");

    if (string.IsNullOrWhiteSpace(hasloAdministratora))
    {
        return;
    }

    const string emailAdministratora =
        "admin@medibook.pl";

    var administrator =
        await userManager.FindByEmailAsync(
            emailAdministratora);

    if (administrator == null)
    {
        administrator = new Uzytkownik
        {
            UserName = emailAdministratora,
            Email = emailAdministratora,
            Imie = "Administrator",
            Nazwisko = "MediBook"
        };

        var wynik =
            await userManager.CreateAsync(
                administrator,
                hasloAdministratora);

        if (!wynik.Succeeded)
        {
            return;
        }
    }

    if (!await userManager.IsInRoleAsync(
        administrator,
        "Administrator"))
    {
        await userManager.AddToRoleAsync(
            administrator,
            "Administrator");
    }
}