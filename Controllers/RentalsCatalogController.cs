using CarDealership.Data;
using CarDealership.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Controllers
{
    public class RentalsCatalogController : Controller
    {
        private readonly ApplicationDbContext _db;

        public RentalsCatalogController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index(string? q)
        {
            var carsQuery = _db.Cars.AsNoTracking()
                .Where(c => c.Status == Car.StatusType.Available &&
                            c.Type == Car.ListingType.ForRent);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                carsQuery = carsQuery.Where(c =>
                    c.Brand.Contains(q) ||
                    c.Model.Contains(q) ||
                    c.Engine.Contains(q) ||
                    c.FuelType.Contains(q));
            }

            var cars = await carsQuery
                .OrderByDescending(c => c.Year)
                .ThenBy(c => c.Brand)
                .ToListAsync();

            ViewBag.Query = q;
            return View(cars);
        }
        public async Task<IActionResult> Details(int id)
        {
            var car = await _db.Cars.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id &&
                                         c.Type == Car.ListingType.ForRent);

            if (car == null) return NotFound();
            return View(car);
        }
    }
}
