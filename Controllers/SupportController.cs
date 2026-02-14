using CarDealership.Data;
using CarDealership.Models;
using CarDealership.Models.ViewModels.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarDealership.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SupportController> _logger;

        public SupportController(ApplicationDbContext db, ILogger<SupportController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SupportTicketCreateVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupportTicketCreateVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var ticket = new SupportTicket
            {
                UserId = userId,
                Subject = vm.Subject.Trim(),
                Description = vm.Description.Trim(),
                Category = vm.Category,
                Priority = vm.Priority,
                Status = TicketStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _db.SupportTickets.Add(ticket);
            await _db.SaveChangesAsync();

            var msg = new SupportTicketMessage
            {
                TicketId = ticket.Id,
                AuthorUserId = userId,
                IsAdmin = false,
                Message = ticket.Description,
                CreatedAt = DateTime.UtcNow
            };

            _db.SupportTicketMessages.Add(msg);
            await _db.SaveChangesAsync();

            _logger.LogInformation("SupportTicket CREATED: TicketId={TicketId} UserId={UserId} Subject={Subject} At={AtUtc}",
                ticket.Id, userId, ticket.Subject, ticket.CreatedAt);

            TempData["Success"] = $"Тикетът е изпратен успешно. №{ticket.Id}";
            return RedirectToAction(nameof(My));
        }

        [HttpGet]
        public async Task<IActionResult> My()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var rows = await _db.SupportTickets.AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Id)
                .Select(t => new SupportTicketRowVm
                {
                    Id = t.Id,
                    CreatedAt = t.CreatedAt,
                    Subject = t.Subject,
                    Category = t.Category,
                    Priority = t.Priority,
                    Status = t.Status
                })
                .ToListAsync();

            return View(rows);
        }
    }
}
