namespace NyayaSetu.Models
{
    public static class AppRoles
    {
        public const string Citizen = "Citizen";
        public const string DepartmentOfficer = "DepartmentOfficer";
        public const string LegalAidOfficer = "LegalAidOfficer";
        public const string SuperAdmin = "SuperAdmin";

        public static readonly string[] All = new[]
        {
            Citizen, DepartmentOfficer, LegalAidOfficer, SuperAdmin
        };
    }

    public enum ApplicationStatus
    {
        Submitted = 0,
        UnderReview = 1,
        DocumentsRequired = 2,
        Verified = 3,
        Approved = 4,
        Rejected = 5,
        Resolved = 6
    }

    public enum ComplaintPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    public enum ComplaintStatus
    {
        Registered = 0,
        Assigned = 1,
        InProgress = 2,
        Escalated = 3,
        Resolved = 4,
        Closed = 5
    }

    public enum LegalAidStatus
    {
        Submitted = 0,
        EligibilityCheck = 1,
        Assigned = 2,
        ConsultationScheduled = 3,
        AssistanceInProgress = 4,
        Completed = 5,
        Rejected = 6
    }

    public enum DocumentCategory
    {
        IdentityProof = 0,
        IncomeCertificate = 1,
        LegalAffidavit = 2,
        CourtNotice = 3,
        SupportingEvidence = 4,
        Other = 5
    }

    public enum DocumentVerificationStatus
    {
        Pending = 0,
        Verified = 1,
        Rejected = 2
    }

    public enum AppointmentStatus
    {
        Requested = 0,
        Confirmed = 1,
        Completed = 2,
        Cancelled = 3
    }

    public enum NotificationType
    {
        Application = 0,
        Document = 1,
        Complaint = 2,
        LegalAid = 3,
        Appointment = 4,
        System = 5
    }
}
