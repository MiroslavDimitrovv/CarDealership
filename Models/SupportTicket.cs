using System.ComponentModel.DataAnnotations;

namespace CarDealership.Models
{
    public class SupportTicket
    {
        public List<SupportTicketMessage> Messages { get; set; } = new();
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [StringLength(120, MinimumLength = 3)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(4000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public TicketCategory Category { get; set; } = TicketCategory.General;

        [Required]
        public TicketPriority Priority { get; set; } = TicketPriority.Normal;

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        [StringLength(4000)]
        public string? AdminNote { get; set; }
    }

    public enum TicketCategory
    {
        General = 0,
        Account = 1,
        Rentals = 2,
        Sales = 3,
        Payments = 4,
        Bug = 5
    }

    public enum TicketPriority
    {
        Low = 0,
        Normal = 1,
        High = 2
    }

    public enum TicketStatus
    {
        Open = 0,
        InProgress = 1,
        Closed = 2
    }
}
