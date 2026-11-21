using Microsoft.EntityFrameworkCore;
using NyayaSetu.Data;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;

namespace NyayaSetu.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string? userId, string? userEmail, string action, string entityName, string? entityId, string? details, string? ipAddress = null)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetRecentLogsAsync(int take = 100)
        {
            return await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(take)
                .ToListAsync();
        }
    }
}
