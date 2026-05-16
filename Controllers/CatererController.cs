using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hearty_Bites.Data;
using Hearty_Bites.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;   
using Microsoft.AspNetCore.Hosting; 

namespace Hearty_Bites.Controllers
{
    [Authorize(Roles = "Caterer")]
    public class CatererController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CatererController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task< IActionResult >Index()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var shop = _context.Caterers
            .Include(c => c.MenuItems.Where(m => !m.IsDeleted))
            .FirstOrDefault(c => c.UserId == userId);
            
            if (shop == null) return RedirectToAction("CreateProfile");
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
        public async Task<IActionResult> AddFood(MenuItem menuItem, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var caterer = _context.Caterers.FirstOrDefault(c => c.UserId == userId);

            if (caterer == null) return NotFound();
            
            if (ModelState.IsValid)
            {
                
                if (imageFile != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\menu-items");

                    if (!Directory.Exists(productPath)) Directory.CreateDirectory(productPath);

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    menuItem.ImageUrl = @"\images\menu-items\" + fileName;
                }

                menuItem.CatererId = caterer.Id;
                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();

               
                return RedirectToAction("ManageCustomizations", new { id = menuItem.Id });
            }
            return View(menuItem);
            
        }

        [HttpGet]
        public async Task<IActionResult> EditFood(int id)
        {
            var food = await _context.MenuItems
            .Include(m => m.CustomizationGroups)
            .FirstOrDefaultAsync(m => m.Id == id);


            if (food == null) return NotFound();
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var caterer = await _context.Caterers.FirstOrDefaultAsync(c => c.UserId == userId);

            if (!User.IsInRole("Admin") && (caterer == null || food.CatererId != caterer.Id))
            {
                return Forbid(); 
            }
            return View(food);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFood(int id, MenuItem model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingFood = await _context.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                    if (existingFood == null) return NotFound();
                    
                    if (imageFile != null)
                    {
                        string wwwRootPath = _webHostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string productPath = Path.Combine(wwwRootPath, @"images\menu-items");

                
                    if (!string.IsNullOrEmpty(existingFood.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, existingFood.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath)) System.IO.File.Exists(oldImagePath);
                    }

                
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    model.ImageUrl = @"\images\menu-items\" + fileName;
                    }
                    else
                    {
                        model.ImageUrl = existingFood.ImageUrl;
                    }
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Something went wrong while updating the food.");
                }

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
