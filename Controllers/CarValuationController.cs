using CarDealership.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CarDealership.Controllers
{
	public class CarValuationController : Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View(new CarValuationVm());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(CarValuationVm model)
		{
			if (!ModelState.IsValid)
				return View(model);

			await Task.Delay(200);

			model.EstimatedPrice =
				model.Year >= 2020 ? 22000 :
				model.Year >= 2015 ? 16000 :
				9000;

			model.IsCalculated = true;
			return View(model);
		}
	}
}
