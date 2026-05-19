using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hearty_Bites.Data;
using Hearty_Bites.Models;

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

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var shop = await _context.Caterers
                .Include(c => c.MenuItems.Where(m => !m.IsDeleted))
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (shop == null) return RedirectToAction("CreateProfile");

            ViewBag.TotalDishes = shop.MenuItems.Count;

            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.CatererId == shop.Id && o.Status == "Delivered")
                .SumAsync(o => o.TotalAmount);

            var comments = await _context.Comments
                .Where(c => c.CatererId == shop.Id && !c.IsDeleted)
                .ToListAsync();

            ViewBag.AvgRating = comments.Any() ? comments.Average(c => c.StarRating) : 5.0;

            return View(shop);
        }

        [HttpGet]
        public IActionResult CreateProfile() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> CreateProfile(Caterer caterer, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                caterer.UserId = userId;

                
                if (imageFile != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string profilePath = Path.Combine(wwwRootPath, @"images\caterers\profiles");

                    if (!Directory.Exists(profilePath))
                    {
                        Directory.CreateDirectory(profilePath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(profilePath, fileName), FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    
                    caterer.ImageUrl = @"/images/caterers/profiles/" + fileName;
                }

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
        public IActionResult AddFood() => View();

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

                TempData["SuccessMessage"] = "New catering menu package has been successfully added!";
                return RedirectToAction(nameof(Index));
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
                            if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var food = await _context.MenuItems.FindAsync(id);
            if (food != null)
            {
                food.IsDeleted = true; // Soft delete kuralı uygulandı
                _context.Update(food);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        
        public async Task<IActionResult> IncomingOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var caterer = await _context.Caterers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (caterer == null) return RedirectToAction("CreateProfile");

            var orders = await _context.Orders
                .Where(o => o.CatererId == caterer.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = newStatus;

            _context.LogEntries.Add(new Hearty_Bites.Models.LogEntry
            {
                Timestamp = DateTime.Now,
                Level = "INFO",
                Message = $"Order Subsystem: Order #{order.Id} status updated to [{newStatus}] by Caterer."
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IncomingOrders));
        }

        public async Task<IActionResult> Reviews()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var caterer = await _context.Caterers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (caterer == null) return RedirectToAction("CreateProfile");

            var reviews = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.CatererId == caterer.Id && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(reviews);
        }

        public async Task<IActionResult> ManageCustomizations(int id)
        {
            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            if (menuItem == null) return NotFound();

            var existingGroups = await _context.CustomizationGroups
                .Include(g => g.Options)
                .Where(g => g.MenuItemId == id)
                .ToListAsync();

            ViewBag.CustomizationGroups = existingGroups;
            return View(menuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomizationOption(int menuItemId, string groupName, string optionName)
        {
            var group = await _context.CustomizationGroups
                .FirstOrDefaultAsync(g => g.MenuItemId == menuItemId && g.GroupName == groupName);
            if (group == null)
            {
                group = new CustomizationGroup { GroupName = groupName, MenuItemId = menuItemId };
                _context.CustomizationGroups.Add(group);
                await _context.SaveChangesAsync();
            }

            var option = new CustamizationOption { OptionName = optionName, CustomizationGroupId = group.Id };
            _context.CustamizationOptions.Add(option);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Option saved successfully: {groupName} -> {optionName}";
            return RedirectToAction(nameof(Index));
        }
    }
}