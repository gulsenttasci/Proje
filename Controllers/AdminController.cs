using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hearty_Bites.Data;
using Hearty_Bites.Models;

namespace Hearty_Bites.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalShops = await _context.Caterers.CountAsync(c => !c.IsDeleted);
            ViewBag.TotalFoods = await _context.MenuItems.CountAsync(m => !m.IsDeleted);
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalLogs = await _context.LogEntries.CountAsync();

            ViewBag.AllCaterers = await _context.Caterers.Where(c => !c.IsDeleted).Take(5).ToListAsync();
            ViewBag.AllMenuItems = await _context.MenuItems.Include(m => m.Caterer).Where(m => !m.IsDeleted).Take(5).ToListAsync();
            ViewBag.AllComments = await _context.Comments.Include(c => c.User).Include(c => c.Caterer).Where(c => !c.IsDeleted).Take(5).ToListAsync();

            return View();
        }

        
        public async Task<IActionResult> Orders(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;

            int totalOrders = await _context.Orders.CountAsync();
            var allOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalOrders / pageSize);
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            return View(allOrders);
        }

        
        public async Task<IActionResult> ManageMenus(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;

            int totalMenus = await _context.MenuItems.CountAsync(m => !m.IsDeleted);
            var menus = await _context.MenuItems
                .Include(m => m.Caterer)
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalMenus / pageSize);
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            return View(menus);
        }

        
        public async Task<IActionResult> ManageComments(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;

            int totalComments = await _context.Comments.CountAsync(c => !c.IsDeleted);
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Caterer)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalComments / pageSize);
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            return View(comments);
        }

        public async Task<IActionResult> Logs(string searchTerm, int pageNumber = 1, int pageSize = 15)
        {
            if (pageNumber < 1) pageNumber = 1;
            var query = _context.LogEntries.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(l => l.Message.Contains(searchTerm) || l.Level.Contains(searchTerm));
            }

            int totalLogs = await query.CountAsync();
            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            return View(logs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                comment.IsDeleted = true;
                _context.LogEntries.Add(new Hearty_Bites.Models.LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = "WARNING",
                    Message = $"Admin Subsystem: Comment #{id} deleted due to moderation policy."
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Comment successfully removed.";
            }
            return RedirectToAction(nameof(ManageComments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenuImage(int id)
        {
            var menu = await _context.MenuItems.FindAsync(id);
            if (menu != null && !string.IsNullOrEmpty(menu.ImageUrl))
            {
                try
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    var imagePath = Path.Combine(wwwRootPath, menu.ImageUrl.TrimStart('/').Replace('/', '\\'));
                    if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);

                    menu.ImageUrl = null;
                    _context.Update(menu);
                    _context.LogEntries.Add(new Hearty_Bites.Models.LogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = "WARNING",
                        Message = $"Admin Subsystem: Cover image for Menu #{id} cleared by Admin."
                    });
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex) { ModelState.AddModelError("", ex.Message); }
            }
            return RedirectToAction(nameof(ManageMenus));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var menu = await _context.MenuItems.FindAsync(id);
            if (menu != null)
            {
                menu.IsDeleted = true;
                _context.LogEntries.Add(new Hearty_Bites.Models.LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = "WARNING",
                    Message = $"Admin Subsystem: Menu Item #{id} soft-deleted."
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageMenus));
        }
    }
}