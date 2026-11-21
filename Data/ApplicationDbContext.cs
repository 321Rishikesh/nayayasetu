using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NyayaSetu.Models;

namespace NyayaSetu.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Department> Departments => Set<Department>();
        public DbSet<OfficerProfile> OfficerProfiles => Set<OfficerProfile>();
        public DbSet<ApplicationCategory> ApplicationCategories => Set<ApplicationCategory>();
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<ApplicationHistory> ApplicationHistories => Set<ApplicationHistory>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Complaint> Complaints => Set<Complaint>();
        public DbSet<LegalAidRequest> LegalAidRequests => Set<LegalAidRequest>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<EscalationRule> EscalationRules => Set<EscalationRule>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Department>(entity =>
            {
                entity.HasIndex(d => d.Code).IsUnique();
                entity.HasIndex(d => d.Name).IsUnique();
            });

            builder.Entity<OfficerProfile>(entity =>
            {
                entity.HasIndex(o => o.UserId).IsUnique();
                entity.HasIndex(o => o.EmployeeCode).IsUnique();
                entity.HasOne(o => o.User)
                      .WithOne(u => u.OfficerProfile)
                      .HasForeignKey<OfficerProfile>(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(o => o.Department)
                      .WithMany(d => d.Officers)
                      .HasForeignKey(o => o.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ApplicationCategory>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
                entity.HasOne(c => c.DefaultDepartment)
                      .WithMany()
                      .HasForeignKey(c => c.DefaultDepartmentId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Application>(entity =>
            {
                entity.HasIndex(a => a.TrackingId).IsUnique();
                entity.HasIndex(a => a.Status);
                entity.HasIndex(a => a.SubmissionDate);

                entity.HasOne(a => a.Citizen)
                      .WithMany(u => u.SubmittedApplications)
                      .HasForeignKey(a => a.CitizenId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.AssignedOfficer)
                      .WithMany(u => u.AssignedApplications)
                      .HasForeignKey(a => a.AssignedOfficerId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.Department)
                      .WithMany(d => d.Applications)
                      .HasForeignKey(a => a.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Category)
                      .WithMany(c => c.Applications)
                      .HasForeignKey(a => a.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ApplicationHistory>(entity =>
            {
                entity.HasIndex(h => h.ApplicationId);
                entity.HasOne(h => h.Application)
                      .WithMany(a => a.History)
                      .HasForeignKey(h => h.ApplicationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(h => h.ChangedByUser)
                      .WithMany()
                      .HasForeignKey(h => h.ChangedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Document>(entity =>
            {
                entity.HasIndex(d => d.StoredFileName).IsUnique();
                entity.HasOne(d => d.Application)
                      .WithMany(a => a.Documents)
                      .HasForeignKey(d => d.ApplicationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.LegalAidRequest)
                      .WithMany(l => l.Documents)
                      .HasForeignKey(d => d.LegalAidRequestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.UploadedByUser)
                      .WithMany()
                      .HasForeignKey(d => d.UploadedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.VerifiedByUser)
                      .WithMany()
                      .HasForeignKey(d => d.VerifiedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Complaint>(entity =>
            {
                entity.HasIndex(c => c.ReferenceNumber).IsUnique();
                entity.HasIndex(c => new { c.Status, c.DueDate });

                entity.HasOne(c => c.Citizen)
                      .WithMany(u => u.SubmittedComplaints)
                      .HasForeignKey(c => c.CitizenId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Department)
                      .WithMany(d => d.Complaints)
                      .HasForeignKey(c => c.DepartmentId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(c => c.AssignedOfficer)
                      .WithMany()
                      .HasForeignKey(c => c.AssignedOfficerId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<LegalAidRequest>(entity =>
            {
                entity.HasIndex(l => l.ReferenceNumber).IsUnique();
                entity.HasOne(l => l.Citizen)
                      .WithMany(u => u.LegalAidRequests)
                      .HasForeignKey(l => l.CitizenId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.AssignedOfficer)
                      .WithMany()
                      .HasForeignKey(l => l.AssignedOfficerId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Appointment>(entity =>
            {
                entity.HasIndex(a => a.ScheduledDateTime);
                entity.HasOne(a => a.Citizen)
                      .WithMany()
                      .HasForeignKey(a => a.CitizenId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Officer)
                      .WithMany()
                      .HasForeignKey(a => a.OfficerId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.LegalAidRequest)
                      .WithMany(l => l.Appointments)
                      .HasForeignKey(a => a.LegalAidRequestId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Notification>(entity =>
            {
                entity.HasIndex(n => new { n.UserId, n.IsRead });
                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Announcement>(entity =>
            {
                entity.HasIndex(a => new { a.IsPublished, a.PublishedAt });
                entity.HasOne(a => a.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(a => a.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(a => a.Timestamp);
                entity.HasIndex(a => a.EntityName);
                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<EscalationRule>(entity =>
            {
                entity.HasIndex(e => e.Priority).IsUnique();
            });
        }
    }
}
