using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(400)]
        public string Summary { get; set; } = string.Empty;

        [Required, StringLength(4000)]
        public string Content { get; set; } = string.Empty;

        [StringLength(80)]
        public string Category { get; set; } = "General";

        public bool IsPublished { get; set; } = true;
        public bool IsPinned { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser? CreatedByUser { get; set; }

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
    }
}
