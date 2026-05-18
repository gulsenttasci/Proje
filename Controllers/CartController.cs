using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Hearty_Bites.Data; 
using Hearty_Bites.Models;

namespace Hearty_Bites.Controllers
{
    
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            
            var cartItems = await _context.CartItems
                .Include(c => c.MenuItem)
                    .ThenInclude(m => m.Caterer)
                .Where(c => c.UserId == userId)
                .ToListAsync();

           
            decimal cartTotal = cartItems.Sum(item => item.UnitPrice * item.Quantity);
            ViewBag.CartTotal = cartTotal;

            return View(cartItems);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddToCart(int menuItemId, int quantity, string customizationSummary, decimal unitPrice)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            string finalSummary = string.IsNullOrEmpty(customizationSummary) ? "Standard" : customizationSummary;
            
            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == menuItemId);
            if (menuItem == null)
            {
                return NotFound();
            }

            decimal actualPrice = (decimal)menuItem.Price;
            
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.MenuItemId == menuItemId && c.CustomizationSummary == finalSummary);

            if (existingItem != null)
            {
                existingItem.Quantity = quantity >= 10 ? quantity : 50; 
                existingItem.UnitPrice = actualPrice;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                
                var cartItem = new CartItem
                {
                    UserId = userId,
                    MenuItemId = menuItemId,
                    Quantity = quantity > 10 ? quantity : 50,
                    UnitPrice = actualPrice,
                    CustomizationSummary = finalSummary
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "The catering package has been successfully added to your cart!";
            return RedirectToAction("Index"); 
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (item != null)
            {
                
                if (quantity >= 10)
                {
                    item.Quantity = quantity;
                    _context.CartItems.Update(item);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = " The number of guests has been successfully updated.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Catering events must have a minimum of 10 people!";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = " The selected package has been removed from your cart.";
            }
            return RedirectToAction(nameof(Index));
        }

       
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.CartItems
                .Include(c => c.MenuItem)
                    .ThenInclude(m => m.Caterer)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            decimal cartTotal = cartItems.Sum(item => item.UnitPrice * item.Quantity);
            ViewBag.CartTotal = cartTotal;

            return View(cartItems);
        }
        
    }
}