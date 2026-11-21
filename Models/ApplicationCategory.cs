using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class ApplicationCategory
    {
        public int Id { get; set; }

        [Required, StringLength(140)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int? DefaultDepartmentId { get; set; }
        public Department? DefaultDepartment { get; set; }

        [Range(1, 365)]
        public int EstimatedProcessingDays { get; set; } = 15;

        public bool IsActive { get; set; } = true;

        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}
