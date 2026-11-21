using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class EscalationRule
    {
        public int Id { get; set; }

        public ComplaintPriority Priority { get; set; }

        [Range(1, 720)]
        public int ResolutionHours { get; set; } = 72;

        public bool AutoEscalateEnabled { get; set; } = true;

        [StringLength(250)]
        public string EscalationNote { get; set; } = "Automatically escalated after exceeding SLA deadline.";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
