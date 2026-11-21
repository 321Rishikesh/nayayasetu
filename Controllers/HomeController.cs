using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NyayaSetu.Data;
using NyayaSetu.Interfaces;
using NyayaSetu.ViewModels;

namespace NyayaSetu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IApplicationService _applications;
        private readonly IDepartmentService _departments;
        private readonly ILocalizationService _loc;

        public HomeController(
            ApplicationDbContext context,
            IApplicationService applications,
            IDepartmentService departments,
            ILocalizationService loc)
        {
            _context = context;
            _applications = applications;
            _departments = departments;
            _loc = loc;
        }

        public async Task<IActionResult> Index()
        {
            var depts = await _departments.GetAllAsync(activeOnly: true);
            var vm = new HomeIndexViewModel
            {
                Departments = depts,
                Categories = await _departments.GetCategoriesAsync(activeOnly: true),
                Announcements = await _context.Announcements
                    .Where(a => a.IsPublished)
                    .OrderByDescending(a => a.IsPinned)
                    .ThenByDescending(a => a.PublishedAt)
                    .Take(5)
                    .ToListAsync(),
                TotalApplicationsProcessed = await _context.Applications.CountAsync(),
                ActiveDepartmentsCount = depts.Count
            };
            return View(vm);
        }

        public IActionResult About() => View();

        public async Task<IActionResult> Services(string? q)
        {
            var categories = await _departments.GetCategoriesAsync(activeOnly: true);
            if (!string.IsNullOrWhiteSpace(q))
            {
                categories = categories.Where(c =>
                    c.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(q, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            ViewBag.Query = q;
            return View(categories);
        }

        public IActionResult LegalAidInfo() => View();

        public IActionResult Contact() => View();

        [HttpGet]
        public async Task<IActionResult> Track(string? trackingId)
        {
            var vm = new TrackApplicationViewModel
            {
                TrackingId = trackingId,
                Searched = !string.IsNullOrWhiteSpace(trackingId)
            };

            if (vm.Searched)
            {
                vm.Application = await _applications.GetByTrackingIdAsync(trackingId!);
            }

            return View(vm);
        }

        [HttpPost]
        public IActionResult SetLanguage(string lang, string? returnUrl)
        {
            var normalized = _loc.NormalizeLanguage(lang);
            Response.Cookies.Append("NyayaSetu.Lang", normalized, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            });
            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }

        public IActionResult Error() => View();
    }
}
