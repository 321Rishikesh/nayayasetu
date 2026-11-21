using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class ApplicationHistory
    {
        public int Id { get; set; }

        [Required]
        public int ApplicationId { get; set; }
        public Application? Application { get; set; }

        public ApplicationStatus Status { get; set; }

        [StringLength(1500)]
        public string? Remarks { get; set; }

        public bool IsVisibleToCitizen { get; set; } = true;

        [Required]
        public string ChangedByUserId { get; set; } = string.Empty;
        public ApplicationUser? ChangedByUser { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
