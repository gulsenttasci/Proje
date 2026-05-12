using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hearty_Bites.Data;
using Hearty_Bites.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Hearty_Bites.Controllers
{
    [Authorize(Roles = "Caterer")]
    public class CatererController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatererController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var shop = _context.Caterers
            .Include(c => c.MenuItems)
            .FirstOrDefault(c => c.UserId == userId);

            return View(shop);
        }


        [HttpGet]
        public IActionResult CreateProfile()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(Caterer caterer)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                caterer.UserId = userId;


                if (ModelState.IsValid)
                {
                    _context.Caterers.Add(caterer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(caterer);
        }


        [HttpGet]
        public IActionResult AddFood()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFood(MenuItem menuItem)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var caterer = _context.Caterers.FirstOrDefault(c => c.UserId == userId);

            if (caterer != null)
            {

                menuItem.CatererId = caterer.Id;

                if (ModelState.IsValid)
                {
                    _context.MenuItems.Add(menuItem);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(menuItem);


        }

        [HttpGet]
        public async Task<IActionResult> EditFood(int id)
        {
            var food = await _context.MenuItems.FindAsync(id);
            if (food == null) return NotFound();
    
            return View(food);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFood(MenuItem model)
        {
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var food = await _context.MenuItems.FindAsync(id);
            if (food != null)
            {
                _context.MenuItems.Remove(food);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }

}
