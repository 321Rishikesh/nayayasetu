using Microsoft.EntityFrameworkCore;
using NyayaSetu.Data;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;
using NyayaSetu.ViewModels;

namespace NyayaSetu.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminAnalyticsViewModel> GetAdminAnalyticsAsync()
        {
            var applications = await _context.Applications.Include(a => a.Department).Include(a => a.AssignedOfficer).ToListAsync();
            var complaints = await _context.Complaints.ToListAsync();
            var departments = await _context.Departments.ToListAsync();
            var officers = await _context.OfficerProfiles.Include(o => o.User).ToListAsync();

            var resolvedApps = applications.Where(a => a.ResolvedAt.HasValue).ToList();
            double avgDays = resolvedApps.Count > 0
                ? Math.Round(resolvedApps.Average(a => (a.ResolvedAt!.Value - a.SubmissionDate).TotalDays), 1)
                : 0;

            // Monthly trend for last 6 months
            var now = DateTime.UtcNow;
            var monthLabels = new List<string>();
            var monthCounts = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                var target = now.AddMonths(-i);
                monthLabels.Add(target.ToString("MMM yyyy"));
                monthCounts.Add(applications.Count(a => a.SubmissionDate.Year == target.Year && a.SubmissionDate.Month == target.Month));
            }

            var deptLabels = departments.Select(d => d.Name).ToList();
            var deptCounts = departments.Select(d => applications.Count(a => a.DepartmentId == d.Id)).ToList();

            var officerNames = officers.Select(o => o.User?.FullName ?? o.EmployeeCode).ToList();
            var officerLoads = officers.Select(o => applications.Count(a =>
                a.AssignedOfficerId == o.UserId &&
                a.Status != ApplicationStatus.Approved &&
                a.Status != ApplicationStatus.Rejected &&
                a.Status != ApplicationStatus.Resolved)).ToList();

            return new AdminAnalyticsViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalApplications = applications.Count,
                PendingApplications = applications.Count(a => a.Status is ApplicationStatus.Submitted or ApplicationStatus.UnderReview or ApplicationStatus.DocumentsRequired),
                ResolvedApplications = applications.Count(a => a.Status is ApplicationStatus.Approved or ApplicationStatus.Resolved),
                TotalComplaints = complaints.Count,
                ResolvedComplaints = complaints.Count(c => c.Status is ComplaintStatus.Resolved or ComplaintStatus.Closed),
                EscalatedComplaints = complaints.Count(c => c.IsEscalated),
                TotalLegalAidRequests = await _context.LegalAidRequests.CountAsync(),
                AverageProcessingDays = avgDays,
                DepartmentLabels = deptLabels,
                DepartmentApplicationCounts = deptCounts,
                MonthlyLabels = monthLabels,
                MonthlyApplicationCounts = monthCounts,
                OfficerLabels = officerNames,
                OfficerActiveWorkloads = officerLoads
            };
        }
    }
}
