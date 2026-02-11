using CarDealership.Data;
using CarDealership.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarDealership.Controllers
{
    [Authorize]
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public RentalsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> My()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var rentals = await _db.Rentals.AsNoTracking()
                .Include(r => r.Car)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(rentals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var rental = await _db.Rentals
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (rental == null) return NotFound();

            var todayUtc = DateTime.UtcNow.Date;

            if (rental.StartDate.Date <= todayUtc)
            {
                TempData["Error"] = "Не можеш да отмениш наем, който вече е започнал.";
                return RedirectToAction(nameof(My));
            }

            rental.Status = Rental.RentalStatus.Cancelled;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Наемът беше отменен.";
            return RedirectToAction(nameof(My));
        }
    }
}
