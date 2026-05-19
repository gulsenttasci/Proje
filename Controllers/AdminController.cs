using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hearty_Bites.Data;
using Hearty_Bites.Models;
using System.Collections.Generic;

namespace Hearty_Bites.Controllers
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            
            ViewBag.TotalShops = await _context.Caterers.CountAsync();
            ViewBag.TotalFoods = await _context.MenuItems.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalLogs = await _context.LogEntries.CountAsync();

            
            ViewBag.AllCaterers = await _context.Caterers.ToListAsync();
            ViewBag.AllMenuItems = await _context.MenuItems.ToListAsync();

            return View();
        }

        public async Task<IActionResult> Orders()
        {
            var allOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(allOrders);
        }

        public async Task<IActionResult> Logs(string searchTerm, int pageNumber = 1, int pageSize = 10)
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

            int totalPages = (int)Math.Ceiling((double)totalLogs / pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < totalPages;

            return View(logs);
        }

    }
}