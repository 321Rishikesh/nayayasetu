using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [StringLength(150)]
        public string? UserEmail { get; set; }

        [Required, StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [StringLength(64)]
        public string? EntityId { get; set; }

        [StringLength(2000)]
        public string? Details { get; set; }

        [StringLength(64)]
        public string? IpAddress { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
