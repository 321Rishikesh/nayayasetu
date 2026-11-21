using NyayaSetu.Models;

namespace NyayaSetu.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Department> Departments { get; set; } = new();
        public List<ApplicationCategory> Categories { get; set; } = new();
        public List<Announcement> Announcements { get; set; } = new();
        public int TotalApplicationsProcessed { get; set; }
        public int ActiveDepartmentsCount { get; set; }
    }

    public class TrackApplicationViewModel
    {
        public string? TrackingId { get; set; }
        public Application? Application { get; set; }
        public bool Searched { get; set; }
    }

    public class AdminUsersViewModel
    {
        public List<UserRoleItem> Users { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
    }

    public class UserRoleItem
    {
        public ApplicationUser User { get; set; } = null!;
        public string Role { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
    }
}
