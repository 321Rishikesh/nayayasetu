using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;
using NyayaSetu.ViewModels;

namespace NyayaSetu.Controllers
{
    [Authorize]
    public class ComplaintsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IComplaintService _complaints;
        private readonly IDepartmentService _departments;
        private readonly IAuditService _audit;

        public ComplaintsController(
            UserManager<ApplicationUser> userManager,
            IComplaintService complaints,
            IDepartmentService departments,
            IAuditService audit)
        {
            _userManager = userManager;
            _complaints = complaints;
            _departments = departments;
            _audit = audit;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            List<Complaint> list;
            if (User.IsInRole(AppRoles.SuperAdmin))
                list = await _complaints.GetAllComplaintsAsync();
            else if (User.IsInRole(AppRoles.DepartmentOfficer))
                list = await _complaints.GetOfficerComplaintsAsync(userId);
            else
                list = await _complaints.GetCitizenComplaintsAsync(userId);

            return View(list);
        }

        [Authorize(Roles = AppRoles.Citizen), HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new CreateComplaintViewModel
            {
                Departments = await _departments.GetAllAsync(activeOnly: true)
            });
        }

        [Authorize(Roles = AppRoles.Citizen), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateComplaintViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = await _departments.GetAllAsync(activeOnly: true);
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;
            var c = await _complaints.RegisterAsync(userId, model.Subject, model.Category, model.Description, model.Priority, model.DepartmentId);
            await _audit.LogAsync(userId, User.Identity?.Name, "RegisterComplaint", "Complaint", c.Id.ToString(), $"Logged {c.ReferenceNumber}");
            TempData["Success"] = $"Grievance registered! Reference: {c.ReferenceNumber}.";
            return RedirectToAction(nameof(Details), new { id = c.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var c = await _complaints.GetByIdAsync(id);
            if (c == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            bool authorized = User.IsInRole(AppRoles.SuperAdmin) ||
                              c.CitizenId == userId ||
                              c.AssignedOfficerId == userId;
            if (!authorized) return Forbid();

            return View(c);
        }

        [Authorize(Roles = $"{AppRoles.DepartmentOfficer},{AppRoles.SuperAdmin}"), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ComplaintStatus status, string? resolutionRemarks)
        {
            var userId = _userManager.GetUserId(User)!;
            await _complaints.UpdateStatusAsync(id, status, resolutionRemarks, userId);
            await _audit.LogAsync(userId, User.Identity?.Name, "UpdateComplaintStatus", "Complaint", id.ToString(), $"Status -> {status}");
            TempData["Success"] = $"Grievance updated to {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
