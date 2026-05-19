using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Hearty_Bites.Data;
using Hearty_Bites.Models;
using Hearty_Bites.Services;
using System.Text;


namespace Hearty_Bites.Controllers
{

    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        public CartController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
            
            int validQuantity = quantity >= 10 ? quantity : 50;
            if (existingItem != null)
            {
                existingItem.Quantity += validQuantity;
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ProcessOrder(DateTime eventDate, string deliveryAddress, string cardHolderName, string cardNumber, string expirationDate, string cvv)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;


            if (eventDate < DateTime.Now)
            {
                TempData["ErrorMessage"] = "The event date cannot be in the past!";
                return RedirectToAction(nameof(Checkout));
            }

            if (string.IsNullOrEmpty(deliveryAddress) || deliveryAddress.Length < 10)
            {
                TempData["ErrorMessage"] = "Please enter a valid and detailed delivery address.";
                return RedirectToAction(nameof(Checkout));
            }


            var cleanCardNumber = cardNumber?.Replace(" ", "");
            if (cleanCardNumber?.Length != 16 || string.IsNullOrEmpty(cvv) || cvv.Length != 3)
            {
                TempData["ErrorMessage"] = "Payment Authorization Failed! Invalid card numbers or CVV format.";
                return RedirectToAction(nameof(Checkout));
            }


            var cartItems = await _context.CartItems
                .Include(c => c.MenuItem)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Cannot process order.";
                return RedirectToAction("Index");
            }

            var firstItem = cartItems.FirstOrDefault();
            int actualCatererId = firstItem?.MenuItem?.CatererId ?? 0;

            if (actualCatererId == 0)
            {
                TempData["ErrorMessage"] = "System Error: The selected menu package does not have a valid Caterer assigned. Please re-add the item to your cart.";
                return RedirectToAction("Index");
            }

            var currentCaterer = await _context.Caterers.FirstOrDefaultAsync(c => c.Id == actualCatererId);
            if (currentCaterer == null)
            {
                TempData["ErrorMessage"] = "System Error: Caterer profile not found in database.";
                return RedirectToAction("Index");
            }

            decimal totalAmount = cartItems.Sum(item => item.UnitPrice * item.Quantity);


            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                EventDate = eventDate,
                DeliveryAddress = deliveryAddress,
                TotalAmount = totalAmount,
                Status = "Confirmed",
                CatererId = actualCatererId,
                Caterer = currentCaterer
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();


            _context.LogEntries.Add(new Hearty_Bites.Models.LogEntry
            {
                Timestamp = DateTime.Now,
                Level = "SUCCESS",
                Message = $"Core Engine: Order #{order.Id} safely persisted to storage. Transaction volume: {order.TotalAmount:N2} ₺. Target destination registered: {order.DeliveryAddress}",
                UserId = userId
            });

            await _context.SaveChangesAsync();

            StringBuilder menuSummaryBuilder = new StringBuilder();

            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    CustomizationSummary = item.CustomizationSummary
                };
                _context.OrderItems.Add(orderItem);


                menuSummaryBuilder.Append($"- {item.MenuItem?.FoodName} (Qty: {item.Quantity}) ");
                if (!string.IsNullOrEmpty(item.CustomizationSummary))
                {
                    menuSummaryBuilder.Append($"[{item.CustomizationSummary}]");
                }
                menuSummaryBuilder.Append("<br>");
            }

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            string userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "customer@gmail.com";
            if (string.IsNullOrEmpty(userEmail) || !userEmail.Contains("@"))
            {
                userEmail = "gulsentasci47@gmail.com";
            }
            string customerName = cardHolderName ?? "Valued Client";
            string assignedCaterer = currentCaterer.ShopName ?? "Independent Caterer Node";
            string finalMenuText = menuSummaryBuilder.ToString();

            var catererUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentCaterer.UserId);
            string catererEmail = catererUser?.Email ?? "thecaterist.test.node@gmail.com";
            if (string.IsNullOrEmpty(catererEmail) || !catererEmail.Contains("@"))
            {
                catererEmail = "gulsentasci47@gmail.com";
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendOrderContractEmailAsync(userEmail, order, customerName, assignedCaterer, finalMenuText);

                    await _emailService.SendOrderContractEmailAsync(catererEmail, order, customerName, assignedCaterer, finalMenuText);
                }
                catch (Exception ex)
                {    
                    //test icin sonra deletedet
                    //Console.WriteLine($"--> Background Email Logging: {ex.Message}");
                    Console.WriteLine($"Non-Fatal Warning: Email delivery paused. Reason: {ex.Message}");


                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"--> 🔴 Gmail Sunucu Detayı: {ex.InnerException.Message}");
                    }
                }
            });


            return RedirectToAction(nameof(OrderSuccess), new { id = order.Id });
        }



        [Authorize]
        public async Task<IActionResult> OrderSuccess(int id)
        {

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


    }
}