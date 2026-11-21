using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;
using NyayaSetu.ViewModels;

namespace NyayaSetu.Controllers
{
    [Authorize(Policy = "CitizenOnly")]
    public class CitizenController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationService _applications;
        private readonly IComplaintService _complaints;
        private readonly ILegalAidService _legalAid;
        private readonly INotificationService _notifications;

        public CitizenController(
            UserManager<ApplicationUser> userManager,
            IApplicationService applications,
            IComplaintService complaints,
            ILegalAidService legalAid,
            INotificationService notifications)
        {
            _userManager = userManager;
            _applications = applications;
            _complaints = complaints;
            _legalAid = legalAid;
            _notifications = notifications;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var apps = await _applications.GetCitizenApplicationsAsync(user.Id);
            var complaints = await _complaints.GetCitizenComplaintsAsync(user.Id);
            var aidRequests = await _legalAid.GetCitizenRequestsAsync(user.Id);
            var notifications = await _notifications.GetUserNotificationsAsync(user.Id, 6);
            var appointments = await _legalAid.GetAppointmentsForUserAsync(user.Id, isOfficer: false);

            var vm = new CitizenDashboardViewModel
            {
                CitizenName = user.FullName,
                TotalApplications = apps.Count,
                PendingApplications = apps.Count(a => a.Status is ApplicationStatus.Submitted or ApplicationStatus.UnderReview or ApplicationStatus.DocumentsRequired),
                ApprovedApplications = apps.Count(a => a.Status is ApplicationStatus.Approved or ApplicationStatus.Resolved),
                RejectedApplications = apps.Count(a => a.Status == ApplicationStatus.Rejected),
                ActiveComplaints = complaints.Count(c => c.Status is not ComplaintStatus.Resolved and not ComplaintStatus.Closed),
                LegalAidRequestsCount = aidRequests.Count,
                RecentApplications = apps.Take(5).ToList(),
                RecentNotifications = notifications,
                UpcomingAppointments = appointments.Where(a => a.Status != AppointmentStatus.Cancelled).Take(4).ToList()
            };

            return View(vm);
        }
    }
}
