using System.Diagnostics;
using Hearty_Bites.Data;
using Microsoft.AspNetCore.Mvc;
using UrbanSpoon.Models;
using Microsoft.EntityFrameworkCore;

namespace UrbanSpoon.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task< IActionResult> Index()
    {
        var allCaterers = await _context.Caterers.ToListAsync();
        return View(allCaterers);
    }

    public async Task<IActionResult> ShopDetails(int id)
    {
        var caterer = await _context.Caterers
            .Include(c => c.MenuItems) 
            .FirstOrDefaultAsync(c => c.Id == id);

        if (caterer == null) return NotFound();

        return View(caterer);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
