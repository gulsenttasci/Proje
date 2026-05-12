using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hearty_Bites.Data;
using Hearty_Bites.Models;
using System.Security.Claims;

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

            var shop = _context.Caterers.FirstOrDefault(c => c.UserId == userId);

            return View(shop);
        }

        // 1. Sayfayı Görüntülemek İçin (GET)
        [HttpGet]
        public IActionResult CreateProfile()
        {
            return View();
        }

        // 2. Formu Gönderince Veritabanına Kaydetmek İçin (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(Caterer caterer)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                caterer.UserId = userId;

                // Model kurallara uygunsa kaydet
                if (ModelState.IsValid)
                {
                    _context.Caterers.Add(caterer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(caterer);
        }


    }
}