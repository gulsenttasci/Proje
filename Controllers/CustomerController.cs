using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Hearty_Bites.Data;
using Hearty_Bites.Models;

namespace Hearty_Bites.Controllers
{

    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(string category = "All")
        {

            ViewBag.ActiveCategory = category;

            var caterersQuery = _context.Caterers
                .Include(c => c.MenuItems) 
                .Where(c => !c.IsDeleted);

            if (category != "All" && !string.IsNullOrEmpty(category))
            {
                caterersQuery = caterersQuery.Where(c => c.MenuItems.Any(m => m.Category == category && !m.IsDeleted));
            }

            var caterers = await caterersQuery.ToListAsync();

            return View(caterers);
        }


        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var myOrders = await _context.Orders
                .Include(o => o.Caterer)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(myOrders);
        }
    }
}