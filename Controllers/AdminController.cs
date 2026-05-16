using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hearty_Bites.Models;
using System.Linq;
using Hearty_Bites.Data;

namespace Hearty_Bites.Controllers
{
    
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context; 

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            
            var totalShops = _context.Caterers.Count();
            var totalFoods = _context.MenuItems.Count();
            
           
            ViewBag.TotalShops = totalShops;
            ViewBag.TotalFoods = totalFoods;
            
            return View();
        }
    }
}