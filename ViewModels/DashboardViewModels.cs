using NyayaSetu.Models;

namespace NyayaSetu.ViewModels
{
    public class CitizenDashboardViewModel
    {
        public string CitizenName { get; set; } = string.Empty;
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int ActiveComplaints { get; set; }
        public int LegalAidRequestsCount { get; set; }
        public List<Application> RecentApplications { get; set; } = new();
        public List<Notification> RecentNotifications { get; set; } = new();
        public List<Appointment> UpcomingAppointments { get; set; } = new();
    }

    public class OfficerDashboardViewModel
    {
        public string OfficerName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int AssignedApplicationsCount { get; set; }
        public int PendingReviewCount { get; set; }
        public int OverdueDeadlinesCount { get; set; }
        public int AssignedComplaintsCount { get; set; }
        public List<Application> AssignedApplications { get; set; } = new();
        public List<Complaint> AssignedComplaints { get; set; } = new();
    }

    public class LegalAidDashboardViewModel
    {
        public string OfficerName { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int PendingEligibility { get; set; }
        public int ScheduledConsultations { get; set; }
        public int CompletedCases { get; set; }
        public List<LegalAidRequest> Requests { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();
    }

    public class AdminAnalyticsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ResolvedApplications { get; set; }
        public int TotalComplaints { get; set; }
        public int ResolvedComplaints { get; set; }
        public int EscalatedComplaints { get; set; }
        public int TotalLegalAidRequests { get; set; }
        public double AverageProcessingDays { get; set; }
        public List<string> DepartmentLabels { get; set; } = new();
        public List<int> DepartmentApplicationCounts { get; set; } = new();
        public List<string> MonthlyLabels { get; set; } = new();
        public List<int> MonthlyApplicationCounts { get; set; } = new();
        public List<string> OfficerLabels { get; set; } = new();
        public List<int> OfficerActiveWorkloads { get; set; } = new();
    }
}
