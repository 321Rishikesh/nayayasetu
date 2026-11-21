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
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAnalyticsService _analytics;
        private readonly IDepartmentService _departments;
        private readonly IApplicationService _applications;
        private readonly IComplaintService _complaints;
        private readonly IAuditService _audit;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAnalyticsService analytics,
            IDepartmentService departments,
            IApplicationService applications,
            IComplaintService complaints,
            IAuditService audit)
        {
            _context = context;
            _userManager = userManager;
            _analytics = analytics;
            _departments = departments;
            _applications = applications;
            _complaints = complaints;
            _audit = audit;
        }

        public async Task<IActionResult> Dashboard()
        {
            var vm = await _analytics.GetAdminAnalyticsAsync();
            return View(vm);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.OfficerProfile)
                    .ThenInclude(o => o!.Department)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var items = new List<UserRoleItem>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                items.Add(new UserRoleItem
                {
                    User = u,
                    Role = roles.FirstOrDefault() ?? AppRoles.Citizen,
                    DepartmentName = u.OfficerProfile?.Department?.Name
                });
            }

            return View(new AdminUsersViewModel
            {
                Users = items,
                Departments = await _departments.GetAllAsync()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            await _audit.LogAsync(_userManager.GetUserId(User), User.Identity?.Name, "ToggleUserStatus", "ApplicationUser", userId, $"IsActive = {user.IsActive}");
            TempData["Success"] = $"User {user.FullName} is now {(user.IsActive ? "Active" : "Deactivated")}.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(string userId, string role, int? departmentId, string? designation)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !AppRoles.All.Contains(role)) return BadRequest();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);

            if ((role == AppRoles.DepartmentOfficer || role == AppRoles.LegalAidOfficer) && departmentId.HasValue)
            {
                var existing = await _context.OfficerProfiles.FirstOrDefaultAsync(o => o.UserId == userId);
                if (existing == null)
                {
                    _context.OfficerProfiles.Add(new OfficerProfile
                    {
                        UserId = userId,
                        DepartmentId = departmentId.Value,
                        Designation = string.IsNullOrWhiteSpace(designation) ? "Department Officer" : designation.Trim(),
                        EmployeeCode = $"NYS-OFF-{Random.Shared.Next(300, 999)}",
                        MaxWorkload = 25,
                        IsAvailable = true
                    });
                }
                else
                {
                    existing.DepartmentId = departmentId.Value;
                    if (!string.IsNullOrWhiteSpace(designation)) existing.Designation = designation.Trim();
                }
                await _context.SaveChangesAsync();
            }

            await _audit.LogAsync(_userManager.GetUserId(User), User.Identity?.Name, "UpdateUserRole", "ApplicationUser", userId, $"Assigned role {role}");
            TempData["Success"] = $"Updated role for {user.FullName} to {role}.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Departments()
        {
            var list = await _departments.GetAllAsync();
            return View(list);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(string name, string code, string? description, string? headOfDepartment, string? contactEmail)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
            {
                TempData["Error"] = "Department Name and Code are required.";
                return RedirectToAction(nameof(Departments));
            }

            await _departments.CreateAsync(new Department
            {
                Name = name.Trim(),
                Code = code.Trim().ToUpperInvariant(),
                Description = description?.Trim(),
                HeadOfDepartment = headOfDepartment?.Trim(),
                ContactEmail = contactEmail?.Trim(),
                IsActive = true
            });
            await _audit.LogAsync(_userManager.GetUserId(User), User.Identity?.Name, "CreateDepartment", "Department", code, $"Created {name}");
            TempData["Success"] = $"Department '{name}' created.";
            return RedirectToAction(nameof(Departments));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var (success, message) = await _departments.DeleteAsync(id);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Departments));
        }

        public async Task<IActionResult> Applications(ApplicationStatus? status, int? departmentId, string? search)
        {
            var apps = await _applications.GetAllApplicationsAsync(status, departmentId, search);
            var recs = new Dictionary<int, OfficerAssignmentRecommendation?>();
            foreach (var app in apps.Where(a => string.IsNullOrEmpty(a.AssignedOfficerId)))
            {
                recs[app.Id] = await _applications.RecommendOfficerAsync(app.DepartmentId, app.CategoryId);
            }

            var vm = new AdminApplicationListViewModel
            {
                Applications = apps,
                Departments = await _departments.GetAllAsync(),
                Officers = await _context.OfficerProfiles.Include(o => o.User).Include(o => o.Department).ToListAsync(),
                Recommendations = recs,
                SelectedStatus = status,
                SelectedDepartmentId = departmentId,
                Search = search
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignApplication(int applicationId, string officerUserId)
        {
            var adminId = _userManager.GetUserId(User)!;
            await _applications.AssignOfficerAsync(applicationId, officerUserId, adminId);
            await _audit.LogAsync(adminId, User.Identity?.Name, "AssignApplication", "Application", applicationId.ToString(), $"Assigned to officer {officerUserId}");
            TempData["Success"] = "Application assigned to officer.";
            return RedirectToAction(nameof(Applications));
        }

        public async Task<IActionResult> Complaints()
        {
            var vm = new AdminComplaintsViewModel
            {
                Complaints = await _complaints.GetAllComplaintsAsync(),
                Officers = await _context.OfficerProfiles.Include(o => o.User).Include(o => o.Department).ToListAsync(),
                Departments = await _departments.GetAllAsync(),
                EscalationRules = await _complaints.GetEscalationRulesAsync()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignComplaint(int complaintId, string officerUserId, int? departmentId)
        {
            var adminId = _userManager.GetUserId(User)!;
            await _complaints.AssignAsync(complaintId, officerUserId, departmentId, adminId);
            TempData["Success"] = "Grievance assigned to officer.";
            return RedirectToAction(nameof(Complaints));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RunEscalationCheck()
        {
            var count = await _complaints.RunOverdueEscalationCheckAsync();
            await _audit.LogAsync(_userManager.GetUserId(User), User.Identity?.Name, "RunEscalationCheck", "Complaint", "Batch", $"Escalated {count} overdue grievances.");
            TempData["Success"] = $"Overdue escalation check complete. {count} complaint(s) flagged as escalated.";
            return RedirectToAction(nameof(Complaints));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEscalationRule(int id, int resolutionHours, bool autoEscalateEnabled, string escalationNote)
        {
            await _complaints.UpdateEscalationRuleAsync(id, resolutionHours, autoEscalateEnabled, escalationNote);
            TempData["Success"] = "Escalation rule updated.";
            return RedirectToAction(nameof(Complaints));
        }

        public async Task<IActionResult> Announcements()
        {
            var list = await _context.Announcements.OrderByDescending(a => a.PublishedAt).ToListAsync();
            return View(list);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(string title, string summary, string content, string category, bool isPinned)
        {
            var adminId = _userManager.GetUserId(User)!;
            _context.Announcements.Add(new Announcement
            {
                Title = title.Trim(),
                Summary = summary.Trim(),
                Content = content.Trim(),
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
                IsPublished = true,
                IsPinned = isPinned,
                CreatedByUserId = adminId,
                PublishedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            await _audit.LogAsync(adminId, User.Identity?.Name, "CreateAnnouncement", "Announcement", title, "Published announcement");
            TempData["Success"] = "Announcement published.";
            return RedirectToAction(nameof(Announcements));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var ann = await _context.Announcements.FindAsync(id);
            if (ann != null)
            {
                _context.Announcements.Remove(ann);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Announcement deleted.";
            }
            return RedirectToAction(nameof(Announcements));
        }

        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _audit.GetRecentLogsAsync(150);
            return View(logs);
        }
    }
}
