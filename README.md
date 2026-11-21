# NyayaSetu – Smart Ministry of Law and Justice Management System

> **Academic Prototype Notice:** NyayaSetu is an educational software prototype built for academic demonstration. It is **not** affiliated with, endorsed by, or operated by the Government of India or the Ministry of Law and Justice.

## Technology Stack
- **Backend:** C#, ASP.NET Core 8.0 MVC
- **ORM & Database:** Entity Framework Core 8.0, Microsoft SQL Server (LocalDB or full SQL Server)
- **Authentication & Authorization:** ASP.NET Core Identity with 4 Roles (`Citizen`, `DepartmentOfficer`, `LegalAidOfficer`, `SuperAdmin`)
- **Frontend:** HTML5, CSS3, Bootstrap 5, JavaScript, Chart.js, Font Awesome 6, Inter/Poppins Typography

## Exact Commands to Run Locally

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server or SQL Server Express LocalDB
- Optional: EF Core CLI (`dotnet tool install --global dotnet-ef`)

### 1. Configure Connection String
Update `appsettings.json` (`ConnectionStrings:DefaultConnection`) if you use a named SQL Server instance instead of `(localdb)\mssqllocaldb`.

### 2. Restore Packages & Create/Apply EF Core Migration
```bash
cd NyayaSetu
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
```
*(Note: On startup, `DbInitializer.SeedAsync` also ensures the database exists and seeds roles, departments, categories, escalation rules, announcements, and sample records automatically.)*

### 3. Build & Run
```bash
dotnet build
dotnet run
```
Open `https://localhost:7214` or `http://localhost:5214` in your browser.

## Demo Prototype Accounts (Seeded Automatically)
| Role | Email | Password |
| :--- | :--- | :--- |
| **Super Admin** | `admin@nyayasetu.local` | `Admin@123` |
| **Department Officer (DLA)** | `officer@nyayasetu.local` | `Officer@123` |
| **Department Officer (DoJ)** | `officer.doj@nyayasetu.local` | `Officer@123` |
| **Legal Aid Officer** | `legalaid@nyayasetu.local` | `LegalAid@123` |
| **Citizen** | `citizen@nyayasetu.local` | `Citizen@123` |

## Implemented Features
- **Public Portal:** Government-inspired responsive landing page, academic prototype disclosure banner, Ministry departments, Services directory, Legal Aid information with verified official links ([NALSA](https://nalsa.gov.in/), [Tele-Law](https://www.tele-law.in/), [eCourts](https://services.ecourts.gov.in/), [Ministry of Law & Justice](https://lawmin.gov.in/)), Public Application Tracking (`NYS-2026-1001`, `NYS-2026-1002`), Announcements, FAQ, Contact.
- **4 Distinct Role-Based Sidebar Dashboards:** Separate layouts (`_CitizenLayout`, `_OfficerLayout`, `_LegalAidLayout`, `_AdminLayout`) and policy-protected controllers.
- **Application Management & Visual Tracking:** Full lifecycle (`Submitted` → `UnderReview` → `DocumentsRequired` → `Verified` → `Approved` / `Rejected` / `Resolved`), citizen-visible officer remarks, and chronological timeline.
- **Department & Officer Management:** Add/delete departments, manage users, assign roles and departments.
- **Complaint & Grievance Management with Automatic Escalation:** Priority-based SLA deadlines, configurable `EscalationRule` table, background worker (`OverdueEscalationBackgroundService`), and on-demand admin trigger.
- **Legal Aid & Consultation Scheduling:** Eligibility review under Section 12 of the Legal Services Authorities Act, 1987, document uploads, and consultation appointment booking/updates.
- **Secure Private Document Management:** Extension, size (≤5 MB), and binary magic-byte validation; stored outside `wwwroot` under `App_Data/SecureDocuments`; authorized download endpoint only; officer verify/reject workflow with rejection reasons.
- **In-App Notifications & Audit Logs:** Automatic notifications on submissions, status changes, document reviews, complaints, legal aid, and appointments; security audit trail.
- **Super Admin Chart.js Analytics Dashboard:** Department-wise doughnut chart, 6-month trend line chart, officer active workload bar chart, and KPI cards.
- **Advanced Features:**
  - **AI Legal Information Assistant (`/Assistant`):** Verified-source Indian legal terminology & service guide with mandatory non-lawyer disclaimer and official links.
  - **Multilingual UI Switcher:** English, Hindi (हिन्दी), and Punjabi (ਪੰਜਾਬੀ).
  - **Smart Application Assignment:** Recommends the best officer by department, specialization match, and active workload ratio, with admin override.
  - **Smart Document OCR Assistance (`OcrService`):** Built-in PDF text-layer & image legibility pre-check with a clean `IOcrService` interface to plug in Tesseract or Azure AI Document Intelligence.
  - **Accessibility & UX:** Light/Dark mode persistence, High-Contrast toggle, keyboard focus rings, and Web Speech API voice-assisted search.

## Optional Production Enhancements (Not Required for Local Academic Demo)
- External SMTP email delivery for Forgot Password reset links (currently logs token generation in audit trail for offline demo safety).
- Optional external OCR engine (e.g., Tesseract / Azure AI Document Intelligence) plugged into `IOcrService` for scanned image text extraction.
- Optional external LLM API integration inside `IAiAssistantService` if live generative responses beyond the verified legal knowledge base are desired.
