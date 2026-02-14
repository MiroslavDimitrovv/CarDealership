using CarDealership.Data;
using CarDealership.Models;
using CarDealership.Models.ViewModels.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CarDealership.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSupportController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AdminSupportController> _logger;

        public AdminSupportController(ApplicationDbContext db, ILogger<AdminSupportController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, TicketStatus? status)
        {
            q ??= "";
            var term = q.Trim();

            var query =
                from t in _db.SupportTickets.AsNoTracking()
                join u in _db.Users.AsNoTracking() on t.UserId equals u.Id
                join c in _db.Clients.AsNoTracking() on t.UserId equals c.UserId into cc
                from c in cc.DefaultIfEmpty()
                select new
                {
                    t.Id,
                    t.CreatedAt,
                    t.Subject,
                    t.Category,
                    t.Priority,
                    t.Status,
                    Email = u.Email ?? "",
                    FullName = c != null ? c.FullName : null,
                    Phone = c != null ? c.PhoneNumber : (u.PhoneNumber ?? null)
                };

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(x =>
                    x.Subject.Contains(term) ||
                    x.Email.Contains(term) ||
                    (x.FullName ?? "").Contains(term) ||
                    (x.Phone ?? "").Contains(term)
                );
            }

            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            var rows = await query
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            ViewBag.Q = q;
            ViewBag.Status = status;

            return View(rows);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _db.SupportTickets.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == ticket.UserId);
            var client = await _db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == ticket.UserId);

            var messages = await _db.SupportTicketMessages.AsNoTracking()
                .Where(m => m.TicketId == id)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.Messages = messages;

            var vm = new AdminSupportDetailsVm
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                Email = user?.Email ?? "",
                FullName = client?.FullName,
                PhoneNumber = client?.PhoneNumber,

                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,

                Subject = ticket.Subject,
                Description = ticket.Description,

                Category = ticket.Category,
                Priority = ticket.Priority,
                Status = ticket.Status,

                AdminNote = ticket.AdminNote,

                NewStatus = ticket.Status,
                NewPriority = ticket.Priority,
                NewAdminNote = ticket.AdminNote
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AdminSupportDetailsVm vm)
        {
            var ticket = await _db.SupportTickets.FirstOrDefaultAsync(t => t.Id == vm.Id);
            if (ticket == null) return NotFound();

            ticket.Status = vm.NewStatus;
            ticket.Priority = vm.NewPriority;
            ticket.AdminNote = string.IsNullOrWhiteSpace(vm.NewAdminNote) ? null : vm.NewAdminNote.Trim();
            ticket.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Тикетът е обновен.";
            return RedirectToAction(nameof(Details), new { id = vm.Id });
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

            var ticket = await _db.SupportTickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var msg = new SupportTicketMessage
            {
                TicketId = id,
                AuthorUserId = adminId,
                IsAdmin = true,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _db.SupportTicketMessages.Add(msg);

            if (ticket.Status == TicketStatus.Open)
                ticket.Status = TicketStatus.InProgress;

            ticket.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("SupportTicket REPLY: TicketId={TicketId} AdminId={AdminId} At={AtUtc} MsgLen={Len}",
                id, adminId, msg.CreatedAt, msg.Message.Length);

            TempData["Success"] = "Отговорът е записан.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _db.SupportTickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            var msgs = await _db.SupportTicketMessages.Where(m => m.TicketId == id).ToListAsync();
            _db.SupportTicketMessages.RemoveRange(msgs);

            _db.SupportTickets.Remove(ticket);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Тикетът е изтрит.";
            return RedirectToAction(nameof(Index));
        }
    }
}
