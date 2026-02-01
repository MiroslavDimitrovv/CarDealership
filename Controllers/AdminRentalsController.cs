using CarDealership.Data;
using CarDealership.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminRentalsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminRentalsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Occupancy()
        {
            var cars = await _db.Cars
                .Where(c => c.Type == Car.ListingType.ForRent)
                .Include(c => c.Rentals.Where(r => r.Status == Rental.RentalStatus.Active))
                    .ThenInclude(r => r.Client)
                .AsNoTracking()
                .OrderBy(c => c.Brand)
                .ThenBy(c => c.Model)
                .ToListAsync();

            return View(cars);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Release(int rentalId)
        {
            var rental = await _db.Rentals
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null)
                return NotFound();

            if (rental.Status != Rental.RentalStatus.Active)
                return RedirectToAction(nameof(Occupancy));

            rental.Status = Rental.RentalStatus.Completed;

            if (rental.Car != null)
                rental.Status = Rental.RentalStatus.ReleasedByOperator;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Автомобилът беше освободен успешно.";
            return RedirectToAction(nameof(Occupancy));
        }
    }
}
