using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NyayaSetu.Data;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;
using NyayaSetu.ViewModels;

namespace NyayaSetu.Controllers
{
    [Authorize(Policy = "OfficerOnly")]
    public class OfficerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IApplicationService _applications;
        private readonly IComplaintService _complaints;
        private readonly IDocumentService _documents;
        private readonly IAuditService _audit;

        public OfficerController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IApplicationService applications,
            IComplaintService complaints,
            IDocumentService documents,
            IAuditService audit)
        {
            _userManager = userManager;
            _context = context;
            _applications = applications;
            _complaints = complaints;
            _documents = documents;
            _audit = audit;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _context.OfficerProfiles
                .Include(o => o.Department)
                .FirstOrDefaultAsync(o => o.UserId == user.Id);

            var apps = await _applications.GetOfficerApplicationsAsync(user.Id);
            var complaints = await _complaints.GetOfficerComplaintsAsync(user.Id);

            var vm = new OfficerDashboardViewModel
            {
                OfficerName = user.FullName,
                DepartmentName = profile?.Department?.Name ?? "Assigned Department",
                AssignedApplicationsCount = apps.Count,
                PendingReviewCount = apps.Count(a => a.Status is ApplicationStatus.Submitted or ApplicationStatus.UnderReview or ApplicationStatus.DocumentsRequired),
                OverdueDeadlinesCount = apps.Count(a => a.DueDate < DateTime.UtcNow && a.ResolvedAt == null),
                AssignedComplaintsCount = complaints.Count(c => c.Status is not ComplaintStatus.Resolved and not ComplaintStatus.Closed),
                AssignedApplications = apps,
                AssignedComplaints = complaints
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            var app = await _applications.GetByIdAsync(id);
            if (app == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            if (!User.IsInRole(AppRoles.SuperAdmin) && app.AssignedOfficerId != userId)
                return Forbid();

            var vm = new ApplicationReviewViewModel
            {
                Application = app,
                NewStatus = app.Status,
                Remarks = app.OfficerRemarks ?? string.Empty
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ApplicationStatus newStatus, string remarks, bool visibleToCitizen = true)
        {
            var app = await _applications.GetByIdAsync(id);
            if (app == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            if (!User.IsInRole(AppRoles.SuperAdmin) && app.AssignedOfficerId != userId)
                return Forbid();

            await _applications.UpdateStatusAsync(id, newStatus, remarks, visibleToCitizen, userId);
            await _audit.LogAsync(userId, User.Identity?.Name, "UpdateApplicationStatus", "Application", id.ToString(), $"Status -> {newStatus}");
            TempData["Success"] = $"Application {app.TrackingId} updated to {newStatus}.";
            return RedirectToAction(nameof(Review), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyDocument(int documentId, int applicationId, DocumentVerificationStatus status, string? rejectionReason)
        {
            var userId = _userManager.GetUserId(User)!;
            await _documents.VerifyDocumentAsync(documentId, status, rejectionReason, userId);
            await _audit.LogAsync(userId, User.Identity?.Name, "VerifyDocument", "Document", documentId.ToString(), $"Document marked {status}");
            TempData["Success"] = $"Document marked as {status}.";
            return RedirectToAction(nameof(Review), new { id = applicationId });
        }
    }
}
