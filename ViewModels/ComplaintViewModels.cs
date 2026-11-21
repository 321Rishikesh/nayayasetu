using System.ComponentModel.DataAnnotations;
using NyayaSetu.Models;

namespace NyayaSetu.ViewModels
{
    public class CreateComplaintViewModel
    {
        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Category { get; set; } = "Service Delay";

        [Required, StringLength(3000, MinimumLength = 15)]
        public string Description { get; set; } = string.Empty;

        public ComplaintPriority Priority { get; set; } = ComplaintPriority.Medium;

        [Display(Name = "Department (Optional)")]
        public int? DepartmentId { get; set; }

        public List<Department> Departments { get; set; } = new();
    }

    public class AdminComplaintsViewModel
    {
        public List<Complaint> Complaints { get; set; } = new();
        public List<OfficerProfile> Officers { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<EscalationRule> EscalationRules { get; set; } = new();
    }
}
