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
        public async Task<IActionResult> UpdateOffices(int rentalId, OfficeLocation pickupOffice, OfficeLocation returnOffice)
        {
            var rental = await _db.Rentals.FirstOrDefaultAsync(r => r.Id == rentalId);
            if (rental == null)
                return NotFound();

            if (rental.Status != Rental.RentalStatus.Active)
            {
                TempData["Error"] = "Може да променяш офисите само на активен наем.";
                return RedirectToAction(nameof(Occupancy));
            }

            rental.PickupOffice = pickupOffice;
            rental.ReturnOffice = returnOffice;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Офисите са обновени.";
            return RedirectToAction(nameof(Occupancy));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int rentalId)
        {
            var rental = await _db.Rentals.FirstOrDefaultAsync(r => r.Id == rentalId);
            if (rental == null)
                return NotFound();

            if (rental.Status != Rental.RentalStatus.Active)
            {
                TempData["Error"] = "Може да маркираш платено само активен наем.";
                return RedirectToAction(nameof(Occupancy));
            }

            rental.IsPaid = true;
            rental.PaidAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Наемът е маркиран като платен.";
            return RedirectToAction(nameof(Occupancy));
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

       
            // if (rental.PayMethod == Rental.PaymentMethod.CashOnPickup && !rental.IsPaid)
            // {
            //     rental.IsPaid = true;
            //     rental.PaidAt = DateTime.UtcNow;
            // }

            rental.Status = Rental.RentalStatus.ReleasedByOperator;

            if (rental.Car != null)
            { rental.Car.CurrentOffice = rental.ReturnOffice;
                rental.Car.Status = Car.StatusType.Available;
            }
              

            await _db.SaveChangesAsync();

            TempData["Success"] = "Автомобилът беше освободен успешно.";
            return RedirectToAction(nameof(Occupancy));
        }
    }
}
