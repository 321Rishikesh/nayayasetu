using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using NyayaSetu.Models;

namespace NyayaSetu.ViewModels
{
    public class CreateLegalAidViewModel
    {
        [Required, StringLength(120)]
        [Display(Name = "Case Type")]
        public string CaseType { get; set; } = string.Empty;

        [Required, StringLength(3000, MinimumLength = 20)]
        [Display(Name = "Case Summary")]
        public string CaseSummary { get; set; } = string.Empty;

        [Range(0, 100000000)]
        [Display(Name = "Annual Household Income (INR)")]
        public decimal AnnualIncome { get; set; }

        [Required, StringLength(120)]
        [Display(Name = "Eligibility Category (LSA Act, 1987)")]
        public string EligibilityCategory { get; set; } = "Low Income Group (Section 12(h) LSA Act)";

        [Display(Name = "Supporting Proof (Income / ID Certificate)")]
        public IFormFile? SupportingDocument { get; set; }
    }

    public class BookAppointmentViewModel
    {
        public int? LegalAidRequestId { get; set; }

        [Required, StringLength(180)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Purpose { get; set; }

        [Required]
        [Display(Name = "Preferred Date & Time")]
        public DateTime ScheduledDateTime { get; set; } = DateTime.UtcNow.AddDays(2).Date.AddHours(11);

        [Required]
        public string Mode { get; set; } = "Video Call";
    }
}
