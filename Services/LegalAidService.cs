using Microsoft.EntityFrameworkCore;
using NyayaSetu.Data;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;

namespace NyayaSetu.Services
{
    public class LegalAidService : ILegalAidService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notifications;

        public LegalAidService(ApplicationDbContext context, INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        public async Task<LegalAidRequest> SubmitRequestAsync(string citizenId, string caseType, string caseSummary, decimal annualIncome, string eligibilityCategory)
        {
            var count = await _context.LegalAidRequests.CountAsync() + 3001;
            var reference = $"LAID-{DateTime.UtcNow.Year}-{count:D4}";
            while (await _context.LegalAidRequests.AnyAsync(l => l.ReferenceNumber == reference))
            {
                count++;
                reference = $"LAID-{DateTime.UtcNow.Year}-{count:D4}";
            }

            // Preliminary eligibility indicator under Section 12 of the Legal Services Authorities Act, 1987
            bool preliminaryEligible = annualIncome <= 300000m ||
                !eligibilityCategory.Contains("General", StringComparison.OrdinalIgnoreCase);

            var request = new LegalAidRequest
            {
                ReferenceNumber = reference,
                CitizenId = citizenId,
                CaseType = caseType.Trim(),
                CaseSummary = caseSummary.Trim(),
                AnnualIncome = annualIncome,
                EligibilityCategory = eligibilityCategory.Trim(),
                IsEligible = preliminaryEligible,
                Status = LegalAidStatus.Submitted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.LegalAidRequests.Add(request);
            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                citizenId,
                $"Legal Aid Request Submitted ({reference})",
                "Your free legal aid application has been received for eligibility review.",
                NotificationType.LegalAid,
                $"/LegalAid/Details/{request.Id}");

            return request;
        }

        public async Task<LegalAidRequest?> GetByIdAsync(int id)
        {
            return await _context.LegalAidRequests
                .Include(l => l.Citizen)
                .Include(l => l.AssignedOfficer)
                .Include(l => l.Documents)
                .Include(l => l.Appointments)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<List<LegalAidRequest>> GetCitizenRequestsAsync(string citizenId)
        {
            return await _context.LegalAidRequests
                .Include(l => l.AssignedOfficer)
                .Where(l => l.CitizenId == citizenId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LegalAidRequest>> GetAllRequestsAsync(string? assignedOfficerId = null)
        {
            var query = _context.LegalAidRequests
                .Include(l => l.Citizen)
                .Include(l => l.AssignedOfficer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(assignedOfficerId))
                query = query.Where(l => l.AssignedOfficerId == assignedOfficerId || l.AssignedOfficerId == null);

            return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }

        public async Task<bool> UpdateProgressAsync(int requestId, LegalAidStatus status, bool isEligible, string? officerNotes, string officerUserId, string? assignOfficerId = null)
        {
            var req = await _context.LegalAidRequests.FindAsync(requestId);
            if (req == null) return false;

            req.Status = status;
            req.IsEligible = isEligible;
            if (!string.IsNullOrWhiteSpace(officerNotes))
                req.OfficerNotes = officerNotes.Trim();
            if (!string.IsNullOrEmpty(assignOfficerId))
                req.AssignedOfficerId = assignOfficerId;
            else if (string.IsNullOrEmpty(req.AssignedOfficerId))
                req.AssignedOfficerId = officerUserId;

            req.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                req.CitizenId,
                $"Legal Aid Update ({req.ReferenceNumber})",
                $"Status updated to {status}. {(string.IsNullOrWhiteSpace(officerNotes) ? "" : officerNotes.Trim())}",
                NotificationType.LegalAid,
                $"/LegalAid/Details/{req.Id}");

            return true;
        }

        public async Task<Appointment> BookAppointmentAsync(
            string citizenId,
            string? officerId,
            int? legalAidRequestId,
            string subject,
            string? purpose,
            DateTime scheduledDateTime,
            string mode,
            string? locationOrLink)
        {
            var appt = new Appointment
            {
                CitizenId = citizenId,
                OfficerId = officerId,
                LegalAidRequestId = legalAidRequestId,
                Subject = subject.Trim(),
                Purpose = purpose?.Trim(),
                ScheduledDateTime = scheduledDateTime,
                Mode = mode,
                MeetingLocationOrLink = locationOrLink,
                Status = string.IsNullOrEmpty(officerId) ? AppointmentStatus.Requested : AppointmentStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                citizenId,
                "Legal Consultation Scheduled",
                $"Appointment '{appt.Subject}' on {appt.ScheduledDateTime:dd MMM yyyy HH:mm} ({appt.Mode}).",
                NotificationType.Appointment,
                "/LegalAid");

            return appt;
        }

        public async Task<List<Appointment>> GetAppointmentsForUserAsync(string userId, bool isOfficer)
        {
            var query = _context.Appointments
                .Include(a => a.Citizen)
                .Include(a => a.Officer)
                .Include(a => a.LegalAidRequest)
                .AsQueryable();

            query = isOfficer
                ? query.Where(a => a.OfficerId == userId || a.OfficerId == null)
                : query.Where(a => a.CitizenId == userId);

            return await query.OrderBy(a => a.ScheduledDateTime).ToListAsync();
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, AppointmentStatus status, string? meetingLocationOrLink, string? notes, string actorUserId)
        {
            var appt = await _context.Appointments.FindAsync(appointmentId);
            if (appt == null) return false;

            appt.Status = status;
            if (!string.IsNullOrWhiteSpace(meetingLocationOrLink))
                appt.MeetingLocationOrLink = meetingLocationOrLink.Trim();
            if (!string.IsNullOrWhiteSpace(notes))
                appt.Notes = notes.Trim();
            if (string.IsNullOrEmpty(appt.OfficerId))
                appt.OfficerId = actorUserId;

            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                appt.CitizenId,
                $"Consultation {status}",
                $"Your consultation '{appt.Subject}' is now {status}.",
                NotificationType.Appointment,
                "/LegalAid");

            return true;
        }
    }
}
