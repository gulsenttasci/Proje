using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hearty_Bites.Data;
using Hearty_Bites.Models;

namespace Hearty_Bites.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MyOrders(int pageNumber = 1, int pageSize = 5)
        {
            if (pageNumber < 1) pageNumber = 1;

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            
            int totalOrders = await _context.Orders.CountAsync(o => o.UserId == userId);
            decimal totalSpent = await _context.Orders.Where(o => o.UserId == userId).SumAsync(o => o.TotalAmount);

            
            var userOrders = await _context.Orders
                .Include(o => o.Caterer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

           
            var ratedCatererIds = await _context.Comments
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Select(c => c.CatererId)
                .Distinct()
                .ToListAsync();

            int totalPages = (int)Math.Ceiling((double)totalOrders / pageSize);

            
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < totalPages;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.RatedCatererIds = ratedCatererIds; 

            return View(userOrders);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitComment(int catererId, int starRating, string commentText)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (starRating < 1 || starRating > 5) return BadRequest();

            
            var newComment = new Comments
            {
                CatererId = catererId,
                StarRating = starRating,
                CommentText = commentText ?? string.Empty,
                UserId = userId,
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };

            _context.Comments.Add(newComment);

            
            _context.LogEntries.Add(new Hearty_Bites.Models.LogEntry
            {
                Timestamp = DateTime.Now,
                Level = "INFO",
                Message = $"User Subsystem: Caterer #{catererId} successfully received a {starRating}-star evaluation from Client ID: {userId}."
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyOrders));
        }
    }
}