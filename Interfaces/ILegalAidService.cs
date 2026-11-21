using NyayaSetu.Models;

namespace NyayaSetu.Interfaces
{
    public interface ILegalAidService
    {
        Task<LegalAidRequest> SubmitRequestAsync(string citizenId, string caseType, string caseSummary, decimal annualIncome, string eligibilityCategory);
        Task<LegalAidRequest?> GetByIdAsync(int id);
        Task<List<LegalAidRequest>> GetCitizenRequestsAsync(string citizenId);
        Task<List<LegalAidRequest>> GetAllRequestsAsync(string? assignedOfficerId = null);
        Task<bool> UpdateProgressAsync(int requestId, LegalAidStatus status, bool isEligible, string? officerNotes, string officerUserId, string? assignOfficerId = null);
        Task<Appointment> BookAppointmentAsync(string citizenId, string? officerId, int? legalAidRequestId, string subject, string? purpose, DateTime scheduledDateTime, string mode, string? locationOrLink);
        Task<List<Appointment>> GetAppointmentsForUserAsync(string userId, bool isOfficer);
        Task<bool> UpdateAppointmentStatusAsync(int appointmentId, AppointmentStatus status, string? meetingLocationOrLink, string? notes, string actorUserId);
    }
}
