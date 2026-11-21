using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [StringLength(600)]
        public string? Description { get; set; }

        [StringLength(120)]
        public string? HeadOfDepartment { get; set; }

        [EmailAddress, StringLength(150)]
        public string? ContactEmail { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OfficerProfile> Officers { get; set; } = new List<OfficerProfile>();
        public ICollection<Application> Applications { get; set; } = new List<Application>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}
