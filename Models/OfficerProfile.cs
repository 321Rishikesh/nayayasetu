using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class OfficerProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }

        [Required, StringLength(120)]
        public string Designation { get; set; } = string.Empty;

        [Required, StringLength(40)]
        public string EmployeeCode { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Specialization { get; set; }

        [Range(1, 200)]
        public int MaxWorkload { get; set; } = 25;

        public bool IsAvailable { get; set; } = true;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
