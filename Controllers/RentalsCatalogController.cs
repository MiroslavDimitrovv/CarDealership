using CarDealership.Data;
using CarDealership.Models;
using CarDealership.Models.ViewModels;
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

        [HttpGet]
        public async Task<IActionResult> Index(CarFilterVm filter)
        {
            var baseQuery = _db.Cars.AsNoTracking()
                .Where(c => c.Status == Car.StatusType.Available &&
                            c.Type == Car.ListingType.ForRent);

            filter.Brands = await baseQuery.Select(c => c.Brand).Distinct().OrderBy(x => x).ToListAsync();
            filter.FuelTypes = await baseQuery.Select(c => c.FuelType).Distinct().OrderBy(x => x).ToListAsync();
            filter.Transmissions = await baseQuery.Select(c => c.Transmission).Distinct().OrderBy(x => x).ToListAsync();

            var q = baseQuery;

            if (!string.IsNullOrWhiteSpace(filter.Q))
            {
                var term = filter.Q.Trim();
                q = q.Where(c =>
                    c.Brand.Contains(term) ||
                    c.Model.Contains(term) ||
                    c.Engine.Contains(term) ||
                    c.FuelType.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.Brand))
                q = q.Where(c => c.Brand == filter.Brand);

            if (!string.IsNullOrWhiteSpace(filter.FuelType))
                q = q.Where(c => c.FuelType == filter.FuelType);

            if (!string.IsNullOrWhiteSpace(filter.Transmission))
                q = q.Where(c => c.Transmission == filter.Transmission);

            if (filter.YearFrom.HasValue)
                q = q.Where(c => c.Year >= filter.YearFrom.Value);

            if (filter.YearTo.HasValue)
                q = q.Where(c => c.Year <= filter.YearTo.Value);

            if (filter.MileageTo.HasValue)
                q = q.Where(c => c.Mileage <= filter.MileageTo.Value);

            if (filter.HorsePowerFrom.HasValue)
                q = q.Where(c => c.HorsePower >= filter.HorsePowerFrom.Value);

            if (filter.HorsePowerTo.HasValue)
                q = q.Where(c => c.HorsePower <= filter.HorsePowerTo.Value);

            // price for rent
            if (filter.PriceFrom.HasValue)
                q = q.Where(c => c.RentPricePerDay != null && c.RentPricePerDay >= filter.PriceFrom.Value);

            if (filter.PriceTo.HasValue)
                q = q.Where(c => c.RentPricePerDay != null && c.RentPricePerDay <= filter.PriceTo.Value);

            var cars = await q
                .OrderByDescending(c => c.Year)
                .ThenBy(c => c.Brand)
                .ToListAsync();

            var pageVm = new RentalsIndexVm { Filter = filter, Cars = cars };
            return View(pageVm);
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
