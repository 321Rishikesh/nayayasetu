using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class Application
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string TrackingId { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(3000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string CitizenId { get; set; } = string.Empty;
        public ApplicationUser? Citizen { get; set; }

        [Required]
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public ApplicationCategory? Category { get; set; }

        public string? AssignedOfficerId { get; set; }
        public ApplicationUser? AssignedOfficer { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

        [StringLength(1500)]
        public string? OfficerRemarks { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(15);
        public DateTime? ResolvedAt { get; set; }

        public ICollection<ApplicationHistory> History { get; set; } = new List<ApplicationHistory>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
