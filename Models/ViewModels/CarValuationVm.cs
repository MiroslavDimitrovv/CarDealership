using System.ComponentModel.DataAnnotations;

namespace CarDealership.Models.ViewModels
{
    public class CarValuationVm
    {
        [Required]
        public string Brand { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        [Range(1990, 2100)]
        public int Year { get; set; }

        [Range(0, 1_000_000)]
        public int Mileage { get; set; }

        [Required]
        public string FuelType { get; set; } = string.Empty;

        public decimal? EstimatedPrice { get; set; }
        public bool IsCalculated { get; set; }
    }
}
