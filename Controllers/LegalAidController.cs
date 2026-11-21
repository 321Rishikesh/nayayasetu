using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;
using NyayaSetu.ViewModels;

namespace NyayaSetu.Controllers
{
    [Authorize]
    public class LegalAidController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILegalAidService _legalAid;
        private readonly IDocumentService _documents;
        private readonly IAuditService _audit;

        public LegalAidController(
            UserManager<ApplicationUser> userManager,
            ILegalAidService legalAid,
            IDocumentService documents,
            IAuditService audit)
        {
            _userManager = userManager;
            _legalAid = legalAid;
            _documents = documents;
            _audit = audit;
        }

        [Authorize(Roles = AppRoles.Citizen)]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            ViewBag.Appointments = await _legalAid.GetAppointmentsForUserAsync(userId, isOfficer: false);
            var list = await _legalAid.GetCitizenRequestsAsync(userId);
            return View(list);
        }

        [Authorize(Policy = "LegalAidOnly")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var requests = await _legalAid.GetAllRequestsAsync(User.IsInRole(AppRoles.SuperAdmin) ? null : user.Id);
            var appts = await _legalAid.GetAppointmentsForUserAsync(user.Id, isOfficer: true);

            var vm = new LegalAidDashboardViewModel
            {
                OfficerName = user.FullName,
                TotalRequests = requests.Count,
                PendingEligibility = requests.Count(r => r.Status is LegalAidStatus.Submitted or LegalAidStatus.EligibilityCheck),
                ScheduledConsultations = appts.Count(a => a.Status is AppointmentStatus.Requested or AppointmentStatus.Confirmed),
                CompletedCases = requests.Count(r => r.Status == LegalAidStatus.Completed),
                Requests = requests,
                Appointments = appts
            };
            return View(vm);
        }

        [Authorize(Roles = AppRoles.Citizen), HttpGet]
        public IActionResult Create() => View(new CreateLegalAidViewModel());

        [Authorize(Roles = AppRoles.Citizen), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLegalAidViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User)!;
            var req = await _legalAid.SubmitRequestAsync(userId, model.CaseType, model.CaseSummary, model.AnnualIncome, model.EligibilityCategory);

            if (model.SupportingDocument != null)
            {
                var upload = await _documents.UploadAsync(model.SupportingDocument, DocumentCategory.IncomeCertificate, userId, legalAidRequestId: req.Id);
                if (!upload.Success)
                    TempData["Warning"] = $"Request submitted, but document upload failed: {upload.Message}";
            }

            await _audit.LogAsync(userId, User.Identity?.Name, "SubmitLegalAid", "LegalAidRequest", req.Id.ToString(), $"Submitted {req.ReferenceNumber}");
            TempData["Success"] = $"Legal Aid request submitted! Reference: {req.ReferenceNumber}.";
            return RedirectToAction(nameof(Details), new { id = req.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var req = await _legalAid.GetByIdAsync(id);
            if (req == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            bool authorized = User.IsInRole(AppRoles.SuperAdmin) ||
                              User.IsInRole(AppRoles.LegalAidOfficer) ||
                              req.CitizenId == userId;
            if (!authorized) return Forbid();

            return View(req);
        }

        [Authorize(Policy = "LegalAidOnly"), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(int id, LegalAidStatus status, bool isEligible, string? officerNotes)
        {
            var userId = _userManager.GetUserId(User)!;
            await _legalAid.UpdateProgressAsync(id, status, isEligible, officerNotes, userId);
            await _audit.LogAsync(userId, User.Identity?.Name, "UpdateLegalAid", "LegalAidRequest", id.ToString(), $"Status -> {status}");
            TempData["Success"] = "Legal aid request updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = $"{AppRoles.Citizen},{AppRoles.LegalAidOfficer},{AppRoles.SuperAdmin}"), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model, string? citizenIdForOfficer)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please provide valid consultation appointment details.";
                return RedirectToAction(User.IsInRole(AppRoles.Citizen) ? nameof(Index) : nameof(Dashboard));
            }

            var currentUserId = _userManager.GetUserId(User)!;
            bool isOfficer = User.IsInRole(AppRoles.LegalAidOfficer) || User.IsInRole(AppRoles.SuperAdmin);
            var targetCitizenId = isOfficer && !string.IsNullOrEmpty(citizenIdForOfficer) ? citizenIdForOfficer : currentUserId;
            string? officerId = isOfficer ? currentUserId : null;

            await _legalAid.BookAppointmentAsync(
                targetCitizenId,
                officerId,
                model.LegalAidRequestId,
                model.Subject,
                model.Purpose,
                model.ScheduledDateTime,
                model.Mode,
                model.Mode == "Video Call" ? "NyayaSetu Virtual Consultation Room" : "District Legal Aid Desk");

            TempData["Success"] = "Legal consultation appointment scheduled.";
            return RedirectToAction(isOfficer ? nameof(Dashboard) : nameof(Index));
        }

        [Authorize(Policy = "LegalAidOnly"), HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointment(int id, AppointmentStatus status, string? meetingLocationOrLink, string? notes)
        {
            var userId = _userManager.GetUserId(User)!;
            await _legalAid.UpdateAppointmentStatusAsync(id, status, meetingLocationOrLink, notes, userId);
            TempData["Success"] = $"Appointment marked as {status}.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
