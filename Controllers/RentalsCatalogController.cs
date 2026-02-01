using CarDealership.Data;
using CarDealership.Models;
using CarDealership.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            var today = DateTime.Today;

            var baseQuery = _db.Cars.AsNoTracking()
                .Where(c => c.Status == Car.StatusType.Available &&
                            c.Type == Car.ListingType.ForRent);

            baseQuery = baseQuery.Where(c =>
                !_db.Rentals.Any(r =>
                    r.CarId == c.Id &&
                    r.Status == Rental.RentalStatus.Active &&
                    r.StartDate <= today &&
                    r.EndDate >= today));

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
                .FirstOrDefaultAsync(c => c.Id == id && c.Type == Car.ListingType.ForRent);

            if (car == null) return NotFound();

            var booked = await _db.Rentals.AsNoTracking()
                .Where(r => r.CarId == id && r.Status == Rental.RentalStatus.Active)
                .Select(r => new { start = r.StartDate.Date, end = r.EndDate.Date })
                .OrderBy(r => r.start)
                .ToListAsync();

            ViewBag.Booked = booked;

            return View(car);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rent(int carId, DateTime startDate, DateTime endDate, string paymentMethod)
        {
            var car = await _db.Cars.FirstOrDefaultAsync(c => c.Id == carId);

            if (car == null) return NotFound();

            if (car.Type != Car.ListingType.ForRent)
            {
                TempData["Error"] = "Тази кола не е предназначена за наем.";
                return RedirectToAction(nameof(Details), new { id = carId });
            }

            if (car.Status != Car.StatusType.Available)
            {
                TempData["Error"] = "Автомобилът не е наличен за наем в момента.";
                return RedirectToAction(nameof(Details), new { id = carId });
            }

            if (car.RentPricePerDay == null)
            {
                TempData["Error"] = "Липсва цена за наем.";
                return RedirectToAction(nameof(Details), new { id = carId });
            }

            var start = startDate.Date;
            var end = endDate.Date;

            if (start < DateTime.Today)
            {
                TempData["Error"] = "Началната дата не може да е в миналото.";
                return RedirectToAction(nameof(Details), new { id = carId });
            }

            if (end < start)
            {
                TempData["Error"] = "Крайната дата не може да е преди началната.";
                return RedirectToAction(nameof(Details), new { id = carId });
            }

            var days = (end - start).Days + 1;
            if (days < 1 || days > 60)
            {
                TempData["Error"] = "Периодът за наем трябва да е между 1 и 60 дни.";
                return RedirectToAction(nameof(Details), new { id = carId });
            }

            var overlaps = await _db.Rentals.AnyAsync(r =>
                r.CarId == carId &&
                r.Status == Rental.RentalStatus.Active &&
                start <= r.EndDate.Date &&
                end >= r.StartDate.Date);

            if (overlaps)
            {
                TempData["Error"] = "Автомобилът е зает за избрания период. Моля избери други дати.";
                return RedirectToAction(nameof(Details), new { id = carId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            if (!Enum.TryParse<Rental.PaymentMethod>(paymentMethod, ignoreCase: true, out var pm))
                pm = Rental.PaymentMethod.CashOnPickup;

            var rental = new Rental
            {
                CarId = car.Id,
                UserId = userId,
                StartDate = start,
                EndDate = end,
                Days = days,
                PricePerDay = car.RentPricePerDay.Value,
                TotalPrice = car.RentPricePerDay.Value * days,
                Status = Rental.RentalStatus.Active,
                PayMethod = pm
            };

            _db.Rentals.Add(rental);
            await _db.SaveChangesAsync();

            var payText = pm == Rental.PaymentMethod.CardPrepay
                ? "предварително с карта"
                : "в брой при вземане";

            TempData["Success"] = $"Успешно заяви наем за {days} дни. Обща цена: {rental.TotalPrice} €. Плащане: {payText}.";
            return RedirectToAction(nameof(Details), new { id = car.Id });
        }
    }
}
