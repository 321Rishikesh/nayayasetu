using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NyayaSetu.Models
{
    public class LegalAidRequest
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        public string CitizenId { get; set; } = string.Empty;
        public ApplicationUser? Citizen { get; set; }

        public string? AssignedOfficerId { get; set; }
        public ApplicationUser? AssignedOfficer { get; set; }

        [Required, StringLength(120)]
        public string CaseType { get; set; } = string.Empty;

        [Required, StringLength(3000)]
        public string CaseSummary { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AnnualIncome { get; set; }

        [Required, StringLength(120)]
        public string EligibilityCategory { get; set; } = string.Empty;

        public bool IsEligible { get; set; }

        public LegalAidStatus Status { get; set; } = LegalAidStatus.Submitted;

        [StringLength(2000)]
        public string? OfficerNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
