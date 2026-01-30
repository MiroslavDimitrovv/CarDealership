using CarDealership.Models;

namespace CarDealership.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            // ако вече има коли – не добавяме пак
            if (db.Cars.Any()) return;

            var cars = new List<Car>
            {
                new Car
                {
                    Brand="BMW", Model="320d", Year=2017, Mileage=165000,
                    Engine="2.0 Diesel", HorsePower=190, FuelType="Diesel", Transmission="Automatic",
                    Description="Добре поддържана, сервизна история.",
                    Type=Car.ListingType.ForSale, SalePrice=28900m, RentPricePerDay=null,
                    Status=Car.StatusType.Available,
                    ImageFileName = "placeholder.jpg",
                        OwnerId = "seed",




                },
                new Car
                {
                    Brand="Audi", Model="A4", Year=2018, Mileage=142000,
                    Engine="2.0 TDI", HorsePower=150, FuelType="Diesel", Transmission="Manual",
                    Description="Европейски автомобил, реални километри.",
                    Type=Car.ListingType.ForSale, SalePrice=31900m, RentPricePerDay=null,
                    Status=Car.StatusType.Available,
                    ImageFileName = "placeholder.jpg",
                        OwnerId = "seed",


                },
                new Car
                {
                    Brand="Toyota", Model="Corolla", Year=2020, Mileage=68000,
                    Engine="1.8 Hybrid", HorsePower=122, FuelType="Hybrid", Transmission="Automatic",
                    Description="Икономична, идеална за град.",
                    Type=Car.ListingType.ForRent, SalePrice=null, RentPricePerDay=89m,
                    Status=Car.StatusType.Available,
                    ImageFileName = "placeholder.jpg",
                        OwnerId = "seed",


                },
                new Car
                {
                    Brand="Volkswagen", Model="Golf", Year=2019, Mileage=94000,
                    Engine="1.5 TSI", HorsePower=150, FuelType="Petrol", Transmission="Manual",
                    Description="Стегната и комфортна, подходяща за пътувания.",
                    Type=Car.ListingType.ForRent, SalePrice=null, RentPricePerDay=79m,
                    Status=Car.StatusType.Rented,
                    ImageFileName = "placeholder.jpg",
                        OwnerId = "seed",


                },
                new Car
                {
                    Brand="Mercedes-Benz", Model="C200", Year=2016, Mileage=188000,
                    Engine="2.0 Petrol", HorsePower=184, FuelType="Petrol", Transmission="Automatic",
                    Description="Комфорт и класа. Внос, обслужена.",
                    Type=Car.ListingType.ForSale, SalePrice=33900m, RentPricePerDay=null,
                    Status=Car.StatusType.InService,
                    ImageFileName = "placeholder.jpg",
                        OwnerId = "seed",


                },
                new Car
                {
                    Brand="Skoda", Model="Octavia", Year=2021, Mileage=52000,
                    Engine="2.0 TDI", HorsePower=115, FuelType="Diesel", Transmission="Automatic",
                    Description="Практична и икономична.",
                    Type=Car.ListingType.ForRent, SalePrice=null, RentPricePerDay=95m,
                    Status=Car.StatusType.Available,
                    ImageFileName = "placeholder.jpg",
                        OwnerId = "seed",


                },
                new Car
{
    Brand="Ford", Model="Focus", Year=2019, Mileage=88000,
    Engine="1.5 EcoBlue", HorsePower=120, FuelType="Diesel", Transmission="Manual",
    Description="Практичен хечбек, отличен разход.",
    Type=Car.ListingType.ForRent, SalePrice=null, RentPricePerDay=69m,
    Status=Car.StatusType.Available,
    ImageFileName = "placeholder.jpg",
        OwnerId = "seed",


},
new Car
{
    Brand="Hyundai", Model="Tucson", Year=2021, Mileage=54000,
    Engine="1.6 GDI", HorsePower=177, FuelType="Petrol", Transmission="Automatic",
    Description="SUV в отлично състояние, просторен и комфортен.",
    Type=Car.ListingType.ForSale, SalePrice=45900m, RentPricePerDay=null,
    Status=Car.StatusType.Available,
    ImageFileName = "placeholder.jpg",
        OwnerId = "seed",


},
new Car
{
    Brand="Peugeot", Model="508", Year=2020, Mileage=76000,
    Engine="1.6 PureTech", HorsePower=180, FuelType="Petrol", Transmission="Automatic",
    Description="Елегантен седан, много добро оборудване.",
    Type=Car.ListingType.ForRent, SalePrice=null, RentPricePerDay=99m,
    Status=Car.StatusType.Available,
    ImageFileName = "placeholder.jpg",
        OwnerId = "seed",


},
new Car
{
    Brand="Tesla", Model="Model 3", Year=2022, Mileage=31000,
    Engine="Electric", HorsePower=283, FuelType="Electric", Transmission="Automatic",
    Description="Напълно електрически, автопилот, минимален разход.",
    Type=Car.ListingType.ForSale, SalePrice=69900m, RentPricePerDay=null,
    Status=Car.StatusType.Available,
    ImageFileName = "placeholder.jpg",
        OwnerId = "seed",


},
new Car
{
    Brand="Opel", Model="Astra", Year=2018, Mileage=112000,
    Engine="1.4 Turbo", HorsePower=150, FuelType="Petrol", Transmission="Manual",
    Description="Поддържан автомобил, икономичен и надежден.",
    Type=Car.ListingType.ForRent, SalePrice=null, RentPricePerDay=59m,
    Status=Car.StatusType.Unavailable,
    ImageFileName = "placeholder.jpg",
        OwnerId = "seed",


}


            };

            db.Cars.AddRange(cars);
            db.SaveChanges();
        }
    }
}
