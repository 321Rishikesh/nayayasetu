using Microsoft.EntityFrameworkCore;
using NyayaSetu.Data;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;

namespace NyayaSetu.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notifications;

        public ApplicationService(ApplicationDbContext context, INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        public async Task<Application> SubmitAsync(string citizenId, string title, string description, int departmentId, int categoryId)
        {
            var category = await _context.ApplicationCategories.FindAsync(categoryId);
            var processingDays = category?.EstimatedProcessingDays ?? 15;
            var count = await _context.Applications.CountAsync() + 1001;
            var trackingId = $"NYS-{DateTime.UtcNow.Year}-{count:D4}";

            while (await _context.Applications.AnyAsync(a => a.TrackingId == trackingId))
            {
                count++;
                trackingId = $"NYS-{DateTime.UtcNow.Year}-{count:D4}";
            }

            var app = new Application
            {
                TrackingId = trackingId,
                Title = title.Trim(),
                Description = description.Trim(),
                CitizenId = citizenId,
                DepartmentId = departmentId,
                CategoryId = categoryId,
                Status = ApplicationStatus.Submitted,
                SubmissionDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(processingDays)
            };

            _context.Applications.Add(app);
            await _context.SaveChangesAsync();

            _context.ApplicationHistories.Add(new ApplicationHistory
            {
                ApplicationId = app.Id,
                Status = ApplicationStatus.Submitted,
                Remarks = "Application submitted online by citizen.",
                IsVisibleToCitizen = true,
                ChangedByUserId = citizenId,
                ChangedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                citizenId,
                $"Application Submitted ({trackingId})",
                $"Your application '{app.Title}' has been registered. Use tracking ID {trackingId} to monitor progress.",
                NotificationType.Application,
                $"/Applications/Details/{app.Id}");

            return app;
        }

        public async Task<Application?> GetByIdAsync(int id)
        {
            return await _context.Applications
                .Include(a => a.Citizen)
                .Include(a => a.Department)
                .Include(a => a.Category)
                .Include(a => a.AssignedOfficer)
                .Include(a => a.Documents)
                .Include(a => a.History.OrderByDescending(h => h.ChangedAt))
                    .ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Application?> GetByTrackingIdAsync(string trackingId)
        {
            if (string.IsNullOrWhiteSpace(trackingId)) return null;
            var normalized = trackingId.Trim().ToUpperInvariant();
            return await _context.Applications
                .Include(a => a.Department)
                .Include(a => a.Category)
                .Include(a => a.History.Where(h => h.IsVisibleToCitizen).OrderBy(h => h.ChangedAt))
                .FirstOrDefaultAsync(a => a.TrackingId.ToUpper() == normalized);
        }

        public async Task<List<Application>> GetCitizenApplicationsAsync(string citizenId)
        {
            return await _context.Applications
                .Include(a => a.Department)
                .Include(a => a.Category)
                .Where(a => a.CitizenId == citizenId)
                .OrderByDescending(a => a.SubmissionDate)
                .ToListAsync();
        }

        public async Task<List<Application>> GetOfficerApplicationsAsync(string officerId)
        {
            return await _context.Applications
                .Include(a => a.Citizen)
                .Include(a => a.Department)
                .Include(a => a.Category)
                .Include(a => a.Documents)
                .Where(a => a.AssignedOfficerId == officerId)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }

        public async Task<List<Application>> GetAllApplicationsAsync(ApplicationStatus? status = null, int? departmentId = null, string? search = null)
        {
            var query = _context.Applications
                .Include(a => a.Citizen)
                .Include(a => a.Department)
                .Include(a => a.Category)
                .Include(a => a.AssignedOfficer)
                .AsQueryable();

            if (status.HasValue) query = query.Where(a => a.Status == status.Value);
            if (departmentId.HasValue) query = query.Where(a => a.DepartmentId == departmentId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(a => a.TrackingId.Contains(term) || a.Title.Contains(term) || (a.Citizen != null && a.Citizen.FullName.Contains(term)));
            }

            return await query.OrderByDescending(a => a.SubmissionDate).ToListAsync();
        }

        public async Task<bool> AssignOfficerAsync(int applicationId, string officerUserId, string adminUserId)
        {
            var app = await _context.Applications.FindAsync(applicationId);
            if (app == null) return false;

            app.AssignedOfficerId = officerUserId;
            if (app.Status == ApplicationStatus.Submitted)
                app.Status = ApplicationStatus.UnderReview;
            app.UpdatedAt = DateTime.UtcNow;

            _context.ApplicationHistories.Add(new ApplicationHistory
            {
                ApplicationId = app.Id,
                Status = app.Status,
                Remarks = "Application assigned to department officer for review.",
                IsVisibleToCitizen = true,
                ChangedByUserId = adminUserId,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                officerUserId,
                $"New Application Assigned ({app.TrackingId})",
                $"Application '{app.Title}' has been assigned to your review queue.",
                NotificationType.Application,
                $"/Officer/Review/{app.Id}");

            await _notifications.CreateAsync(
                app.CitizenId,
                $"Application Update ({app.TrackingId})",
                $"Your application is now assigned to an officer and is {app.Status}.",
                NotificationType.Application,
                $"/Applications/Details/{app.Id}");

            return true;
        }

        public async Task<bool> UpdateStatusAsync(int applicationId, ApplicationStatus newStatus, string remarks, bool visibleToCitizen, string actorUserId)
        {
            var app = await _context.Applications.FindAsync(applicationId);
            if (app == null) return false;

            app.Status = newStatus;
            if (visibleToCitizen && !string.IsNullOrWhiteSpace(remarks))
                app.OfficerRemarks = remarks.Trim();
            app.UpdatedAt = DateTime.UtcNow;

            if (newStatus is ApplicationStatus.Approved or ApplicationStatus.Rejected or ApplicationStatus.Resolved)
                app.ResolvedAt = DateTime.UtcNow;

            _context.ApplicationHistories.Add(new ApplicationHistory
            {
                ApplicationId = app.Id,
                Status = newStatus,
                Remarks = string.IsNullOrWhiteSpace(remarks) ? $"Status updated to {newStatus}." : remarks.Trim(),
                IsVisibleToCitizen = visibleToCitizen,
                ChangedByUserId = actorUserId,
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                app.CitizenId,
                $"Application {app.TrackingId}: {newStatus}",
                visibleToCitizen && !string.IsNullOrWhiteSpace(remarks)
                    ? remarks.Trim()
                    : $"Your application status has been updated to {newStatus}.",
                NotificationType.Application,
                $"/Applications/Details/{app.Id}");

            return true;
        }

        public async Task<OfficerAssignmentRecommendation?> RecommendOfficerAsync(int departmentId, int categoryId)
        {
            var category = await _context.ApplicationCategories.FindAsync(categoryId);
            var candidates = await _context.OfficerProfiles
                .Include(o => o.User)
                .Where(o => o.DepartmentId == departmentId && o.IsAvailable && o.User != null && o.User.IsActive)
                .ToListAsync();

            if (candidates.Count == 0)
            {
                candidates = await _context.OfficerProfiles
                    .Include(o => o.User)
                    .Where(o => o.IsAvailable && o.User != null && o.User.IsActive)
                    .ToListAsync();
            }

            if (candidates.Count == 0) return null;

            var scored = new List<(OfficerProfile Profile, int ActiveCount, double Score, string Reason)>();
            foreach (var officer in candidates)
            {
                var activeCount = await _context.Applications.CountAsync(a =>
                    a.AssignedOfficerId == officer.UserId &&
                    a.Status != ApplicationStatus.Approved &&
                    a.Status != ApplicationStatus.Rejected &&
                    a.Status != ApplicationStatus.Resolved);

                var loadRatio = officer.MaxWorkload > 0 ? (double)activeCount / officer.MaxWorkload : 1.0;
                var specializationMatch = category != null && !string.IsNullOrWhiteSpace(officer.Specialization) &&
                    category.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Any(word => word.Length > 3 && officer.Specialization.Contains(word, StringComparison.OrdinalIgnoreCase));

                var score = (1.0 - Math.Min(loadRatio, 1.0)) * 70.0 + (specializationMatch ? 30.0 : 0.0);
                var reason = specializationMatch
                    ? $"Matches department & specialization ({officer.Specialization}); active workload {activeCount}/{officer.MaxWorkload}."
                    : $"Lowest active workload in department ({activeCount}/{officer.MaxWorkload}).";

                scored.Add((officer, activeCount, score, reason));
            }

            var best = scored.OrderByDescending(s => s.Score).First();
            return new OfficerAssignmentRecommendation(
                best.Profile.UserId,
                best.Profile.User!.FullName,
                best.Profile.Designation,
                best.ActiveCount,
                best.Profile.MaxWorkload,
                best.Reason);
        }
    }
}
