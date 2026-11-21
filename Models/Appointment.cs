using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public string CitizenId { get; set; } = string.Empty;
        public ApplicationUser? Citizen { get; set; }

        public string? OfficerId { get; set; }
        public ApplicationUser? Officer { get; set; }

        public int? LegalAidRequestId { get; set; }
        public LegalAidRequest? LegalAidRequest { get; set; }

        [Required, StringLength(180)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Purpose { get; set; }

        public DateTime ScheduledDateTime { get; set; }

        [Range(15, 180)]
        public int DurationMinutes { get; set; } = 30;

        [Required, StringLength(40)]
        public string Mode { get; set; } = "Video Call";

        [StringLength(300)]
        public string? MeetingLocationOrLink { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Requested;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
