using CarDealership.Models;
using CarDealership.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q)
        {
            var usersQuery = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                usersQuery = usersQuery.Where(u =>
                    (u.Email ?? "").Contains(q) ||
                    (u.PhoneNumber ?? "").Contains(q)
                );

                usersQuery = usersQuery.Where(u =>
                    (u.Email ?? "").Contains(q) ||
                    (u.PhoneNumber ?? "").Contains(q) ||
                    (EF.Property<string>(u, "FirstName") ?? "").Contains(q) ||
                    (EF.Property<string>(u, "LastName") ?? "").Contains(q)
                );
            }

            var users = await usersQuery
                .OrderBy(u => u.Email)
                .ToListAsync();

            var rows = new List<AdminUserRowVm>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);

                string? firstName = null;
                string? lastName = null;

                try
                {
                    firstName = EF.Property<string>(u, "FirstName");
                    lastName = EF.Property<string>(u, "LastName");
                }
                catch
                {
                }

                rows.Add(new AdminUserRowVm
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber,
                    EmailConfirmed = u.EmailConfirmed,
                    Roles = roles,
                    FirstName = firstName,
                    LastName = lastName
                });
            }

            ViewBag.Q = q ?? "";
            return View(rows);
        }
    }
}
