using System.ComponentModel.DataAnnotations;

namespace NyayaSetu.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public DocumentCategory Category { get; set; } = DocumentCategory.SupportingEvidence;

        public DocumentVerificationStatus VerificationStatus { get; set; } = DocumentVerificationStatus.Pending;

        [StringLength(1000)]
        public string? RejectionReason { get; set; }

        [StringLength(4000)]
        public string? OcrExtractedText { get; set; }

        [StringLength(500)]
        public string? OcrQualityNote { get; set; }

        public int? ApplicationId { get; set; }
        public Application? Application { get; set; }

        public int? LegalAidRequestId { get; set; }
        public LegalAidRequest? LegalAidRequest { get; set; }

        [Required]
        public string UploadedByUserId { get; set; } = string.Empty;
        public ApplicationUser? UploadedByUser { get; set; }

        public string? VerifiedByUserId { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VerifiedAt { get; set; }
    }
}
