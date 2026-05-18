using System.Diagnostics;
using Hearty_Bites.Data;
using Microsoft.AspNetCore.Mvc;
using Hearty_Bites.Models;
using Microsoft.EntityFrameworkCore;

namespace Hearty_Bites.Controllers;

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
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (caterer == null) return NotFound();

        var menus = await _context.MenuItems
            .Include(m => m.CustomizationGroups)
                .ThenInclude(g => g.Options) 
            .Where(m => m.CatererId == id && !m.IsDeleted)
            .ToListAsync();

        ViewBag.Menus = menus;

        return View("ShopDetail",caterer);
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

    [HttpGet]
    public async Task<JsonResult> GetNearbyCaterers(double userLat, double userLng)
    {
   
        var caterers = await _context.Caterers
            .Select(c => new {
                c.Id,
                c.ShopName,
                c.Latitude,
                c.Longitude
            
            }).ToListAsync();

        return Json(caterers);
}
}
