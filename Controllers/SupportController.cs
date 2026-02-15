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
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var ticket = await _db.SupportTickets.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (ticket == null) return NotFound();

            var msgs = await _db.SupportTicketMessages.AsNoTracking()
                .Where(m => m.TicketId == id)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.Messages = msgs;

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string message)
        {
            message = (message ?? "").Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Съобщението не може да е празно.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var ticket = await _db.SupportTickets.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (ticket == null) return NotFound();

            if (ticket.Status == TicketStatus.Closed)
            {
                TempData["Error"] = "Този тикет е затворен и не може да се отговаря.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var msg = new SupportTicketMessage
            {
                TicketId = id,
                AuthorUserId = userId,
                IsAdmin = false,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _db.SupportTicketMessages.Add(msg);

            ticket.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("SupportTicket USER_REPLY: TicketId={TicketId} UserId={UserId} At={AtUtc} MsgLen={Len}",
                id, userId, msg.CreatedAt, msg.Message.Length);

            TempData["Success"] = "Съобщението е изпратено.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
