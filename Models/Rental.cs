using System.ComponentModel.DataAnnotations;

namespace CarDealership.Models
{
    public class Rental : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Кола")]
        public int CarId { get; set; }
        public Car? Car { get; set; }

        [Required]
        [Display(Name = "Клиент")]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        [Required]
        [Display(Name = "Начална дата")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Крайна дата")]
        public DateTime EndDate { get; set; }

        [Range(1, 1000000)]
        [Display(Name = "Обща цена")]
        public decimal TotalPrice { get; set; }
        public enum RentalStatus
        {
            Active = 1,
            Completed = 2,
            Cancelled = 3
        }

        [Display(Name = "Статус")]
        public RentalStatus Status { get; set; } = RentalStatus.Active;
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "Крайната дата трябва да е след началната.",
                    new[] { nameof(EndDate) });
            }

            if (StartDate.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Началната дата не може да е в миналото.",
                    new[] { nameof(StartDate) });
            }

            if (Car != null && Car.Type != Car.ListingType.ForRent)
            {
                yield return new ValidationResult(
                    "Тази кола не е предназначена за наем.",
                    new[] { nameof(CarId) });
            }
        }
    }
}
