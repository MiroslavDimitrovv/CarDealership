namespace CarDealership.Models.ViewModels
{
    public class AdminUserRowVm
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
