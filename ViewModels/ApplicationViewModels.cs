using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;

namespace NyayaSetu.ViewModels
{
    public class CreateApplicationViewModel
    {
        [Required, StringLength(200)]
        [Display(Name = "Application Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Application Category")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Target Department")]
        public int DepartmentId { get; set; }

        [Required, StringLength(3000, MinimumLength = 20)]
        [Display(Name = "Detailed Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Document Category")]
        public DocumentCategory DocumentCategory { get; set; } = DocumentCategory.IdentityProof;

        [Display(Name = "Supporting Document (PDF, JPG, PNG — Max 5 MB)")]
        public IFormFile? SupportingDocument { get; set; }

        public List<Department> Departments { get; set; } = new();
        public List<ApplicationCategory> Categories { get; set; } = new();
    }

    public class ApplicationReviewViewModel
    {
        public Application Application { get; set; } = null!;
        public ApplicationStatus NewStatus { get; set; }
        [Required, StringLength(1500)]
        public string Remarks { get; set; } = string.Empty;
        public bool VisibleToCitizen { get; set; } = true;
    }

    public class AdminApplicationListViewModel
    {
        public List<Application> Applications { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<OfficerProfile> Officers { get; set; } = new();
        public Dictionary<int, OfficerAssignmentRecommendation?> Recommendations { get; set; } = new();
        public ApplicationStatus? SelectedStatus { get; set; }
        public int? SelectedDepartmentId { get; set; }
        public string? Search { get; set; }
    }
}
