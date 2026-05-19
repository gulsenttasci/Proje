using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
           
            if (User.IsInRole("Caterer"))
            {
                
                return RedirectToAction("Index", "Caterer");
            }

            
        }

        return View();
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

        var comments = await _context.Comments
            .Include(c => c.User)
            .Where(c => c.CatererId == id && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();

        ViewBag.Comments = comments;

        return View("ShopDetail", caterer);
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
        var allCaterers = await _context.Caterers
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        var processedCaterers = allCaterers.Select(c =>
        {
            double calculatedDistance = 0;

            if (userLat != 0 && userLng != 0 && c.Latitude.HasValue && c.Longitude.HasValue && c.Latitude != 0 && c.Longitude != 0)
            {
                calculatedDistance = CalculateDistance(userLat, userLng, c.Latitude.Value, c.Longitude.Value);
            }

            return new
            {
                c.Id,
                c.ShopName,
                c.Latitude,
                c.Longitude,
                Distance = calculatedDistance
            };
        }).ToList();

        return Json(processedCaterers);
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double angle) => (Math.PI / 180) * angle;
}