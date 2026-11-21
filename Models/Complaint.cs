using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required, StringLength(3000)]
        public string Description { get; set; } = string.Empty;

        public ComplaintPriority Priority { get; set; } = ComplaintPriority.Medium;

        public ComplaintStatus Status { get; set; } = ComplaintStatus.Registered;

        [Required]
        public string CitizenId { get; set; } = string.Empty;
        public ApplicationUser? Citizen { get; set; }

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public string? AssignedOfficerId { get; set; }
        public ApplicationUser? AssignedOfficer { get; set; }

        [StringLength(2000)]
        public string? ResolutionRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddHours(72);

        public bool IsEscalated { get; set; }
        public DateTime? EscalatedAt { get; set; }

        [StringLength(500)]
        public string? EscalationReason { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}
