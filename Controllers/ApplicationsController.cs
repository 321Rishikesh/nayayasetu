using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;
using NyayaSetu.ViewModels;

namespace NyayaSetu.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationService _applications;
        private readonly IDepartmentService _departments;
        private readonly IDocumentService _documents;
        private readonly IAuditService _audit;

        public ApplicationsController(
            UserManager<ApplicationUser> userManager,
            IApplicationService applications,
            IDepartmentService departments,
            IDocumentService documents,
            IAuditService audit)
        {
            _userManager = userManager;
            _applications = applications;
            _departments = departments;
            _documents = documents;
            _audit = audit;
        }

        [Authorize(Roles = AppRoles.Citizen)]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var apps = await _applications.GetCitizenApplicationsAsync(userId);
            return View(apps);
        }

        [Authorize(Roles = AppRoles.Citizen), HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new CreateApplicationViewModel
            {
                Departments = await _departments.GetAllAsync(activeOnly: true),
                Categories = await _departments.GetCategoriesAsync(activeOnly: true)
            };
            return View(vm);
        }

        [Authorize(Roles = AppRoles.Citizen), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateApplicationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = await _departments.GetAllAsync(activeOnly: true);
                model.Categories = await _departments.GetCategoriesAsync(activeOnly: true);
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;
            var app = await _applications.SubmitAsync(userId, model.Title, model.Description, model.DepartmentId, model.CategoryId);

            if (model.SupportingDocument != null)
            {
                var upload = await _documents.UploadAsync(model.SupportingDocument, model.DocumentCategory, userId, applicationId: app.Id);
                if (!upload.Success)
                {
                    TempData["Warning"] = $"Application {app.TrackingId} submitted, but document upload failed: {upload.Message}";
                }
            }

            await _audit.LogAsync(userId, User.Identity?.Name, "SubmitApplication", "Application", app.Id.ToString(), $"Submitted {app.TrackingId}");
            TempData["Success"] = $"Application submitted! Your Tracking ID is {app.TrackingId}.";
            return RedirectToAction(nameof(Details), new { id = app.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var app = await _applications.GetByIdAsync(id);
            if (app == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            bool isPrivileged = User.IsInRole(AppRoles.SuperAdmin) || User.IsInRole(AppRoles.DepartmentOfficer);
            if (!isPrivileged && app.CitizenId != userId)
                return Forbid();

            return View(app);
        }

        [Authorize(Roles = AppRoles.Citizen), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int applicationId, DocumentCategory category, IFormFile file)
        {
            var app = await _applications.GetByIdAsync(applicationId);
            var userId = _userManager.GetUserId(User)!;
            if (app == null || app.CitizenId != userId) return Forbid();

            var result = await _documents.UploadAsync(file, category, userId, applicationId: applicationId);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Details), new { id = applicationId });
        }
    }
}
