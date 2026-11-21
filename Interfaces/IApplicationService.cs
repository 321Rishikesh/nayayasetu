using NyayaSetu.Models;

namespace NyayaSetu.Interfaces
{
    public record OfficerAssignmentRecommendation(string OfficerUserId, string OfficerName, string Designation, int CurrentActiveWorkload, int MaxWorkload, string Rationale);

    public interface IApplicationService
    {
        Task<Application> SubmitAsync(string citizenId, string title, string description, int departmentId, int categoryId);
        Task<Application?> GetByIdAsync(int id);
        Task<Application?> GetByTrackingIdAsync(string trackingId);
        Task<List<Application>> GetCitizenApplicationsAsync(string citizenId);
        Task<List<Application>> GetOfficerApplicationsAsync(string officerId);
        Task<List<Application>> GetAllApplicationsAsync(ApplicationStatus? status = null, int? departmentId = null, string? search = null);
        Task<bool> AssignOfficerAsync(int applicationId, string officerUserId, string adminUserId);
        Task<bool> UpdateStatusAsync(int applicationId, ApplicationStatus newStatus, string remarks, bool visibleToCitizen, string actorUserId);
        Task<OfficerAssignmentRecommendation?> RecommendOfficerAsync(int departmentId, int categoryId);
    }
}
