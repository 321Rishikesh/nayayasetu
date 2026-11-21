using Microsoft.EntityFrameworkCore;
using NyayaSetu.Data;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;

namespace NyayaSetu.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notifications;

        public ComplaintService(ApplicationDbContext context, INotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        public async Task<Complaint> RegisterAsync(string citizenId, string subject, string category, string description, ComplaintPriority priority, int? departmentId)
        {
            var rule = await _context.EscalationRules.FirstOrDefaultAsync(r => r.Priority == priority);
            var slaHours = rule?.ResolutionHours ?? 72;

            var count = await _context.Complaints.CountAsync() + 2001;
            var reference = $"CMP-{DateTime.UtcNow.Year}-{count:D4}";
            while (await _context.Complaints.AnyAsync(c => c.ReferenceNumber == reference))
            {
                count++;
                reference = $"CMP-{DateTime.UtcNow.Year}-{count:D4}";
            }

            var complaint = new Complaint
            {
                ReferenceNumber = reference,
                Subject = subject.Trim(),
                Category = category.Trim(),
                Description = description.Trim(),
                Priority = priority,
                Status = ComplaintStatus.Registered,
                CitizenId = citizenId,
                DepartmentId = departmentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddHours(slaHours)
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                citizenId,
                $"Grievance Registered ({reference})",
                $"Your grievance '{complaint.Subject}' has been logged with {priority} priority.",
                NotificationType.Complaint,
                $"/Complaints/Details/{complaint.Id}");

            return complaint;
        }

        public async Task<Complaint?> GetByIdAsync(int id)
        {
            return await _context.Complaints
                .Include(c => c.Citizen)
                .Include(c => c.Department)
                .Include(c => c.AssignedOfficer)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Complaint>> GetCitizenComplaintsAsync(string citizenId)
        {
            return await _context.Complaints
                .Include(c => c.Department)
                .Where(c => c.CitizenId == citizenId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Complaint>> GetOfficerComplaintsAsync(string officerId)
        {
            return await _context.Complaints
                .Include(c => c.Citizen)
                .Include(c => c.Department)
                .Where(c => c.AssignedOfficerId == officerId)
                .OrderBy(c => c.DueDate)
                .ToListAsync();
        }

        public async Task<List<Complaint>> GetAllComplaintsAsync(ComplaintStatus? status = null, bool onlyEscalated = false)
        {
            var query = _context.Complaints
                .Include(c => c.Citizen)
                .Include(c => c.Department)
                .Include(c => c.AssignedOfficer)
                .AsQueryable();

            if (status.HasValue) query = query.Where(c => c.Status == status.Value);
            if (onlyEscalated) query = query.Where(c => c.IsEscalated);

            return await query.OrderByDescending(c => c.IsEscalated).ThenByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<bool> AssignAsync(int complaintId, string officerUserId, int? departmentId, string adminUserId)
        {
            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint == null) return false;

            complaint.AssignedOfficerId = officerUserId;
            if (departmentId.HasValue) complaint.DepartmentId = departmentId.Value;
            if (complaint.Status == ComplaintStatus.Registered)
                complaint.Status = ComplaintStatus.Assigned;
            complaint.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                officerUserId,
                $"Grievance Assigned ({complaint.ReferenceNumber})",
                $"Grievance '{complaint.Subject}' has been assigned to you.",
                NotificationType.Complaint,
                $"/Complaints/Details/{complaint.Id}");

            return true;
        }

        public async Task<bool> UpdateStatusAsync(int complaintId, ComplaintStatus status, string? resolutionRemarks, string actorUserId)
        {
            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint == null) return false;

            complaint.Status = status;
            if (!string.IsNullOrWhiteSpace(resolutionRemarks))
                complaint.ResolutionRemarks = resolutionRemarks.Trim();
            complaint.UpdatedAt = DateTime.UtcNow;

            if (status is ComplaintStatus.Resolved or ComplaintStatus.Closed)
            {
                complaint.ResolvedAt = DateTime.UtcNow;
                complaint.IsEscalated = false;
            }

            await _context.SaveChangesAsync();

            await _notifications.CreateAsync(
                complaint.CitizenId,
                $"Grievance {complaint.ReferenceNumber}: {status}",
                !string.IsNullOrWhiteSpace(resolutionRemarks)
                    ? resolutionRemarks.Trim()
                    : $"Your grievance status was updated to {status}.",
                NotificationType.Complaint,
                $"/Complaints/Details/{complaint.Id}");

            return true;
        }

        public async Task<int> RunOverdueEscalationCheckAsync()
        {
            var now = DateTime.UtcNow;
            var rules = await _context.EscalationRules.ToDictionaryAsync(r => r.Priority);

            var overdue = await _context.Complaints
                .Where(c => !c.IsEscalated &&
                            c.Status != ComplaintStatus.Resolved &&
                            c.Status != ComplaintStatus.Closed &&
                            c.DueDate < now)
                .ToListAsync();

            int escalatedCount = 0;
            foreach (var complaint in overdue)
            {
                if (rules.TryGetValue(complaint.Priority, out var rule) && !rule.AutoEscalateEnabled)
                    continue;

                complaint.IsEscalated = true;
                complaint.Status = ComplaintStatus.Escalated;
                complaint.EscalatedAt = now;
                complaint.EscalationReason = rule?.EscalationNote ?? "Automatically escalated after exceeding SLA deadline.";
                complaint.UpdatedAt = now;
                escalatedCount++;

                if (!string.IsNullOrEmpty(complaint.AssignedOfficerId))
                {
                    await _notifications.CreateAsync(
                        complaint.AssignedOfficerId,
                        $"OVERDUE ESCALATION: {complaint.ReferenceNumber}",
                        $"Grievance '{complaint.Subject}' exceeded its deadline and was escalated.",
                        NotificationType.Complaint,
                        $"/Complaints/Details/{complaint.Id}");
                }
            }

            if (escalatedCount > 0)
                await _context.SaveChangesAsync();

            return escalatedCount;
        }

        public async Task<List<EscalationRule>> GetEscalationRulesAsync()
        {
            return await _context.EscalationRules.OrderByDescending(r => r.Priority).ToListAsync();
        }

        public async Task UpdateEscalationRuleAsync(int id, int resolutionHours, bool autoEscalateEnabled, string escalationNote)
        {
            var rule = await _context.EscalationRules.FindAsync(id);
            if (rule == null) return;
            rule.ResolutionHours = resolutionHours;
            rule.AutoEscalateEnabled = autoEscalateEnabled;
            rule.EscalationNote = escalationNote;
            rule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
