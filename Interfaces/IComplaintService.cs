using NyayaSetu.Models;

namespace NyayaSetu.Interfaces
{
    public interface IComplaintService
    {
        Task<Complaint> RegisterAsync(string citizenId, string subject, string category, string description, ComplaintPriority priority, int? departmentId);
        Task<Complaint?> GetByIdAsync(int id);
        Task<List<Complaint>> GetCitizenComplaintsAsync(string citizenId);
        Task<List<Complaint>> GetOfficerComplaintsAsync(string officerId);
        Task<List<Complaint>> GetAllComplaintsAsync(ComplaintStatus? status = null, bool onlyEscalated = false);
        Task<bool> AssignAsync(int complaintId, string officerUserId, int? departmentId, string adminUserId);
        Task<bool> UpdateStatusAsync(int complaintId, ComplaintStatus status, string? resolutionRemarks, string actorUserId);
        Task<int> RunOverdueEscalationCheckAsync();
        Task<List<EscalationRule>> GetEscalationRulesAsync();
        Task UpdateEscalationRuleAsync(int id, int resolutionHours, bool autoEscalateEnabled, string escalationNote);
    }
}
