using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NyayaSetu.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(80)]
        public string? City { get; set; }

        [StringLength(80)]
        public string? State { get; set; }

        [StringLength(12)]
        public string? PinCode { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string PreferredLanguage { get; set; } = "en";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public OfficerProfile? OfficerProfile { get; set; }
        public ICollection<Application> SubmittedApplications { get; set; } = new List<Application>();
        public ICollection<Application> AssignedApplications { get; set; } = new List<Application>();
        public ICollection<Complaint> SubmittedComplaints { get; set; } = new List<Complaint>();
        public ICollection<LegalAidRequest> LegalAidRequests { get; set; } = new List<LegalAidRequest>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
