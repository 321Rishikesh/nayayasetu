using NyayaSetu.Models;

namespace NyayaSetu.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string? userId, string? userEmail, string action, string entityName, string? entityId, string? details, string? ipAddress = null);
        Task<List<AuditLog>> GetRecentLogsAsync(int take = 100);
    }
}
