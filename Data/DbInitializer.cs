using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NyayaSetu.Models;

namespace NyayaSetu.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database and schema exist (use MigrateAsync when migrations are applied)
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            foreach (var role in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Departments
            if (!await context.Departments.AnyAsync())
            {
                context.Departments.AddRange(
                    new Department
                    {
                        Name = "Department of Legal Affairs",
                        Code = "DLA",
                        Description = "Handles legal advice to ministries, notary appointments, litigation coordination, and arbitration matters.",
                        HeadOfDepartment = "Dr. Rajeshwar Sharma",
                        ContactEmail = "dla-demo@nyayasetu.local",
                        IsActive = true
                    },
                    new Department
                    {
                        Name = "Legislative Department",
                        Code = "LEG",
                        Description = "Responsible for drafting legislative bills, statutory rules, gazette notifications, and official language translations.",
                        HeadOfDepartment = "Smt. Meenakshi Deshmukh",
                        ContactEmail = "legislative-demo@nyayasetu.local",
                        IsActive = true
                    },
                    new Department
                    {
                        Name = "Department of Justice",
                        Code = "DOJ",
                        Description = "Oversees judicial infrastructure, eCourts initiatives, Tele-Law access to justice, and legal aid coordination.",
                        HeadOfDepartment = "Shri Harpreet Singh Gill",
                        ContactEmail = "doj-demo@nyayasetu.local",
                        IsActive = true
                    }
                );
                await context.SaveChangesAsync();
            }

            var dla = await context.Departments.FirstAsync(d => d.Code == "DLA");
            var leg = await context.Departments.FirstAsync(d => d.Code == "LEG");
            var doj = await context.Departments.FirstAsync(d => d.Code == "DOJ");

            // 3. Seed Users
            var admin = await EnsureUserAsync(userManager, "admin@nyayasetu.local", "Admin@123", "Aarav Mehta", AppRoles.SuperAdmin, "New Delhi", "Delhi");
            var officerDla = await EnsureUserAsync(userManager, "officer@nyayasetu.local", "Officer@123", "Priya Nair", AppRoles.DepartmentOfficer, "New Delhi", "Delhi");
            var officerDoj = await EnsureUserAsync(userManager, "officer.doj@nyayasetu.local", "Officer@123", "Gurpreet Kaur", AppRoles.DepartmentOfficer, "Ludhiana", "Punjab");
            var legalAidOfficer = await EnsureUserAsync(userManager, "legalaid@nyayasetu.local", "LegalAid@123", "Adv. Vikramaditya Rao", AppRoles.LegalAidOfficer, "Chandigarh", "Punjab");
            var citizen = await EnsureUserAsync(userManager, "citizen@nyayasetu.local", "Citizen@123", "Rohan Verma", AppRoles.Citizen, "Ludhiana", "Punjab");

            // 4. Seed Officer Profiles
            if (!await context.OfficerProfiles.AnyAsync())
            {
                context.OfficerProfiles.AddRange(
                    new OfficerProfile
                    {
                        UserId = officerDla.Id,
                        DepartmentId = dla.Id,
                        Designation = "Senior Legal Affairs Officer",
                        EmployeeCode = "NYS-OFF-101",
                        Specialization = "Notary & Legal Documentation",
                        MaxWorkload = 20,
                        IsAvailable = true
                    },
                    new OfficerProfile
                    {
                        UserId = officerDoj.Id,
                        DepartmentId = doj.Id,
                        Designation = "Access to Justice Coordinator",
                        EmployeeCode = "NYS-OFF-102",
                        Specialization = "eCourts & Citizen Grievances",
                        MaxWorkload = 25,
                        IsAvailable = true
                    },
                    new OfficerProfile
                    {
                        UserId = legalAidOfficer.Id,
                        DepartmentId = doj.Id,
                        Designation = "Panel Legal Aid Officer",
                        EmployeeCode = "NYS-LA-201",
                        Specialization = "Free Legal Aid & Mediation",
                        MaxWorkload = 15,
                        IsAvailable = true
                    }
                );
                await context.SaveChangesAsync();
            }

            // 5. Seed Application Categories
            if (!await context.ApplicationCategories.AnyAsync())
            {
                context.ApplicationCategories.AddRange(
                    new ApplicationCategory { Name = "Notary Certificate Application", Description = "Apply for new notary public registration or renewal.", DefaultDepartmentId = dla.Id, EstimatedProcessingDays = 15 },
                    new ApplicationCategory { Name = "Legal Document Attestation", Description = "Verification and attestation of legal affidavits and certificates.", DefaultDepartmentId = dla.Id, EstimatedProcessingDays = 7 },
                    new ApplicationCategory { Name = "Statutory Rule / Gazette Inquiry", Description = "Request certified copies or clarifications on statutory enactments.", DefaultDepartmentId = leg.Id, EstimatedProcessingDays = 10 },
                    new ApplicationCategory { Name = "Court Record & eCourts Assistance", Description = "Assistance with case status, certified copies, and eFiling queries.", DefaultDepartmentId = doj.Id, EstimatedProcessingDays = 5 },
                    new ApplicationCategory { Name = "Alternative Dispute Resolution (ADR) / Lok Adalat", Description = "Request pre-litigation mediation or Lok Adalat referral.", DefaultDepartmentId = doj.Id, EstimatedProcessingDays = 12 },
                    new ApplicationCategory { Name = "RTI & Transparency Request", Description = "Formal citizen request for public legal administration records.", DefaultDepartmentId = leg.Id, EstimatedProcessingDays = 30 }
                );
                await context.SaveChangesAsync();
            }

            // 6. Seed Escalation Rules
            if (!await context.EscalationRules.AnyAsync())
            {
                context.EscalationRules.AddRange(
                    new EscalationRule { Priority = ComplaintPriority.Critical, ResolutionHours = 24, AutoEscalateEnabled = true, EscalationNote = "Critical grievance exceeded 24-hour SLA." },
                    new EscalationRule { Priority = ComplaintPriority.High, ResolutionHours = 48, AutoEscalateEnabled = true, EscalationNote = "High-priority grievance exceeded 48-hour SLA." },
                    new EscalationRule { Priority = ComplaintPriority.Medium, ResolutionHours = 72, AutoEscalateEnabled = true, EscalationNote = "Medium-priority grievance exceeded 72-hour SLA." },
                    new EscalationRule { Priority = ComplaintPriority.Low, ResolutionHours = 120, AutoEscalateEnabled = true, EscalationNote = "Low-priority grievance exceeded 120-hour SLA." }
                );
                await context.SaveChangesAsync();
            }

            // 7. Seed Sample Applications & History
            if (!await context.Applications.AnyAsync())
            {
                var catNotary = await context.ApplicationCategories.FirstAsync(c => c.Name.Contains("Notary"));
                var catAdr = await context.ApplicationCategories.FirstAsync(c => c.Name.Contains("ADR"));
                var catAttest = await context.ApplicationCategories.FirstAsync(c => c.Name.Contains("Attestation"));

                var app1 = new Application
                {
                    TrackingId = "NYS-2026-1001",
                    Title = "Renewal of Notary Practice Certificate",
                    Description = "Requesting renewal of notary certificate for Ludhiana district with updated bar council registration.",
                    CitizenId = citizen.Id,
                    DepartmentId = dla.Id,
                    CategoryId = catNotary.Id,
                    AssignedOfficerId = officerDla.Id,
                    Status = ApplicationStatus.UnderReview,
                    OfficerRemarks = "Primary identity proof received. Reviewing bar council credentials.",
                    SubmissionDate = DateTime.UtcNow.AddDays(-6),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    DueDate = DateTime.UtcNow.AddDays(9)
                };

                var app2 = new Application
                {
                    TrackingId = "NYS-2026-1002",
                    Title = "Pre-Litigation Mediation Referral (Property Boundary)",
                    Description = "Seeking Lok Adalat / mediation referral to resolve a residential boundary dispute amicably.",
                    CitizenId = citizen.Id,
                    DepartmentId = doj.Id,
                    CategoryId = catAdr.Id,
                    AssignedOfficerId = officerDoj.Id,
                    Status = ApplicationStatus.Approved,
                    OfficerRemarks = "Approved for mediation sitting. Notice issued to both parties.",
                    SubmissionDate = DateTime.UtcNow.AddDays(-18),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4),
                    DueDate = DateTime.UtcNow.AddDays(-6),
                    ResolvedAt = DateTime.UtcNow.AddDays(-4)
                };

                var app3 = new Application
                {
                    TrackingId = "NYS-2026-1003",
                    Title = "Affidavit Attestation for Educational Scholarship",
                    Description = "Requesting official attestation of income and domicile affidavit.",
                    CitizenId = citizen.Id,
                    DepartmentId = dla.Id,
                    CategoryId = catAttest.Id,
                    Status = ApplicationStatus.Submitted,
                    SubmissionDate = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    DueDate = DateTime.UtcNow.AddDays(6)
                };

                context.Applications.AddRange(app1, app2, app3);
                await context.SaveChangesAsync();

                context.ApplicationHistories.AddRange(
                    new ApplicationHistory { ApplicationId = app1.Id, Status = ApplicationStatus.Submitted, Remarks = "Application submitted online by citizen.", IsVisibleToCitizen = true, ChangedByUserId = citizen.Id, ChangedAt = app1.SubmissionDate },
                    new ApplicationHistory { ApplicationId = app1.Id, Status = ApplicationStatus.UnderReview, Remarks = "Assigned to Department of Legal Affairs officer for verification.", IsVisibleToCitizen = true, ChangedByUserId = officerDla.Id, ChangedAt = DateTime.UtcNow.AddDays(-2) },
                    new ApplicationHistory { ApplicationId = app2.Id, Status = ApplicationStatus.Submitted, Remarks = "Application submitted online by citizen.", IsVisibleToCitizen = true, ChangedByUserId = citizen.Id, ChangedAt = app2.SubmissionDate },
                    new ApplicationHistory { ApplicationId = app2.Id, Status = ApplicationStatus.Verified, Remarks = "Documents verified successfully.", IsVisibleToCitizen = true, ChangedByUserId = officerDoj.Id, ChangedAt = DateTime.UtcNow.AddDays(-10) },
                    new ApplicationHistory { ApplicationId = app2.Id, Status = ApplicationStatus.Approved, Remarks = "Approved for mediation sitting. Notice issued to both parties.", IsVisibleToCitizen = true, ChangedByUserId = officerDoj.Id, ChangedAt = DateTime.UtcNow.AddDays(-4) },
                    new ApplicationHistory { ApplicationId = app3.Id, Status = ApplicationStatus.Submitted, Remarks = "Application submitted and awaiting officer assignment.", IsVisibleToCitizen = true, ChangedByUserId = citizen.Id, ChangedAt = app3.SubmissionDate }
                );
                await context.SaveChangesAsync();
            }

            // 8. Seed Sample Complaints
            if (!await context.Complaints.AnyAsync())
            {
                context.Complaints.AddRange(
                    new Complaint
                    {
                        ReferenceNumber = "CMP-2026-2001",
                        Subject = "Delay in Certified Copy Issuance",
                        Category = "Service Delay",
                        Description = "Certified copy requested 14 days ago has not been uploaded to the portal yet.",
                        Priority = ComplaintPriority.High,
                        Status = ComplaintStatus.InProgress,
                        CitizenId = citizen.Id,
                        DepartmentId = doj.Id,
                        AssignedOfficerId = officerDoj.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-4),
                        UpdatedAt = DateTime.UtcNow.AddDays(-2),
                        DueDate = DateTime.UtcNow.AddHours(-12), // Overdue to demonstrate auto-escalation
                        IsEscalated = false
                    },
                    new Complaint
                    {
                        ReferenceNumber = "CMP-2026-2002",
                        Subject = "Portal Document Upload Clarity Feedback",
                        Category = "Technical / Portal",
                        Description = "Need guidance on accepted PDF size for multi-page annexures.",
                        Priority = ComplaintPriority.Low,
                        Status = ComplaintStatus.Resolved,
                        CitizenId = citizen.Id,
                        DepartmentId = dla.Id,
                        AssignedOfficerId = officerDla.Id,
                        ResolutionRemarks = "Clarified 5 MB PDF limit and multi-file upload support.",
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        UpdatedAt = DateTime.UtcNow.AddDays(-8),
                        DueDate = DateTime.UtcNow.AddDays(-5),
                        ResolvedAt = DateTime.UtcNow.AddDays(-8)
                    }
                );
                await context.SaveChangesAsync();
            }

            // 9. Seed Sample Legal Aid Request & Appointment
            if (!await context.LegalAidRequests.AnyAsync())
            {
                var aid = new LegalAidRequest
                {
                    ReferenceNumber = "LAID-2026-3001",
                    CitizenId = citizen.Id,
                    AssignedOfficerId = legalAidOfficer.Id,
                    CaseType = "Civil / Consumer Protection",
                    CaseSummary = "Seeking free legal counsel regarding unfair denial of home appliance warranty claim.",
                    AnnualIncome = 180000m,
                    EligibilityCategory = "Low Income Group (Section 12(h) LSA Act)",
                    IsEligible = true,
                    Status = LegalAidStatus.ConsultationScheduled,
                    OfficerNotes = "Income criteria met. Scheduled introductory consultation.",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                };
                context.LegalAidRequests.Add(aid);
                await context.SaveChangesAsync();

                context.Appointments.Add(new Appointment
                {
                    CitizenId = citizen.Id,
                    OfficerId = legalAidOfficer.Id,
                    LegalAidRequestId = aid.Id,
                    Subject = "Initial Legal Aid Consultation — Consumer Dispute",
                    Purpose = "Review purchase invoice, warranty card, and draft formal legal notice.",
                    ScheduledDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(11),
                    DurationMinutes = 30,
                    Mode = "Video Call",
                    MeetingLocationOrLink = "NyayaSetu Virtual Consultation Room #4",
                    Status = AppointmentStatus.Confirmed,
                    Notes = "Please keep scanned copies of invoice and correspondence ready."
                });
                await context.SaveChangesAsync();
            }

            // 10. Seed Announcements
            if (!await context.Announcements.AnyAsync())
            {
                context.Announcements.AddRange(
                    new Announcement
                    {
                        Title = "National Lok Adalat Schedule — Academic Prototype Notice",
                        Summary = "Citizens can submit pre-litigation mediation requests online through NyayaSetu.",
                        Content = "This academic prototype demonstrates how citizens can request pre-litigation mediation and Lok Adalat referrals. For actual official schedules, please visit NALSA (https://nalsa.gov.in/lok-adalats/).",
                        Category = "Legal Aid",
                        IsPublished = true,
                        IsPinned = true,
                        CreatedByUserId = admin.Id,
                        PublishedAt = DateTime.UtcNow.AddDays(-3)
                    },
                    new Announcement
                    {
                        Title = "Multilingual Support Now Available (English, Hindi, Punjabi)",
                        Summary = "Switch portal interface language anytime from the top navigation bar.",
                        Content = "NyayaSetu now supports English, Hindi (हिन्दी), and Punjabi (ਪੰਜਾਬੀ) interface switching alongside high-contrast accessibility and voice-assisted search.",
                        Category = "System Update",
                        IsPublished = true,
                        IsPinned = false,
                        CreatedByUserId = admin.Id,
                        PublishedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new Announcement
                    {
                        Title = "Tele-Law & Free Legal Aid Awareness Drive",
                        Summary = "Check eligibility criteria under the Legal Services Authorities Act, 1987.",
                        Content = "Eligible citizens including women, children, SC/ST members, senior citizens, persons with disabilities, and low-income households can request free legal aid. Verified official portals: nalsa.gov.in and tele-law.in.",
                        Category = "Public Awareness",
                        IsPublished = true,
                        IsPinned = false,
                        CreatedByUserId = admin.Id,
                        PublishedAt = DateTime.UtcNow.AddDays(-7)
                    }
                );
                await context.SaveChangesAsync();
            }

            // 11. Seed Initial Notifications & Audit Log
            if (!await context.Notifications.AnyAsync())
            {
                context.Notifications.AddRange(
                    new Notification
                    {
                        UserId = citizen.Id,
                        Title = "Application NYS-2026-1001 Under Review",
                        Message = "Your notary renewal application is now under review by the Department of Legal Affairs.",
                        Type = NotificationType.Application,
                        LinkUrl = "/Home/Track?trackingId=NYS-2026-1001",
                        IsRead = false
                    },
                    new Notification
                    {
                        UserId = citizen.Id,
                        Title = "Legal Aid Consultation Confirmed",
                        Message = "Your consultation for LAID-2026-3001 is scheduled via Video Call.",
                        Type = NotificationType.Appointment,
                        LinkUrl = "/LegalAid",
                        IsRead = false
                    }
                );
                await context.SaveChangesAsync();
            }

            if (!await context.AuditLogs.AnyAsync())
            {
                context.AuditLogs.Add(new AuditLog
                {
                    UserId = admin.Id,
                    UserEmail = admin.Email,
                    Action = "SystemSeed",
                    EntityName = "Database",
                    EntityId = "Initial",
                    Details = "Initialized NyayaSetu academic prototype database with roles, departments, and sample records.",
                    IpAddress = "127.0.0.1"
                });
                await context.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role,
            string city,
            string state)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    City = city,
                    State = state,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to seed user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
            return user;
        }
    }
}
