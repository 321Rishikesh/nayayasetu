using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NyayaSetu.Data;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;

namespace NyayaSetu.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IOcrService _ocrService;
        private readonly INotificationService _notifications;

        private static readonly Dictionary<string, byte[][]> FileSignatures = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } }, // %PDF
            [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } }
        };

        public DocumentService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IOcrService ocrService,
            INotificationService notifications)
        {
            _context = context;
            _configuration = configuration;
            _env = env;
            _ocrService = ocrService;
            _notifications = notifications;
        }

        public async Task<(bool Success, string Message, Document? Document)> UploadAsync(
            IFormFile file,
            DocumentCategory category,
            string uploadedByUserId,
            int? applicationId = null,
            int? legalAidRequestId = null)
        {
            if (file == null || file.Length == 0)
                return (false, "Please select a non-empty file to upload.", null);

            var maxBytes = _configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 5 * 1024 * 1024);
            if (file.Length > maxBytes)
                return (false, $"File exceeds the maximum allowed size of {maxBytes / (1024 * 1024)} MB.", null);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!FileSignatures.ContainsKey(ext))
                return (false, "Only PDF, JPG, JPEG, and PNG documents are permitted.", null);

            // Validate magic bytes (file content signature)
            using (var reader = new BinaryReader(file.OpenReadStream()))
            {
                var headerBytes = reader.ReadBytes(8);
                var validSignature = FileSignatures[ext].Any(sig =>
                    headerBytes.Length >= sig.Length && headerBytes.Take(sig.Length).SequenceEqual(sig));

                if (!validSignature)
                    return (false, "File content does not match its declared extension. Upload rejected for security.", null);
            }

            // Store outside wwwroot so private citizen documents are never publicly exposed
            var privateRoot = _configuration.GetValue<string>("FileStorage:PrivateUploadRoot") ?? "App_Data/SecureDocuments";
            var fullFolder = Path.Combine(_env.ContentRootPath, privateRoot);
            Directory.CreateDirectory(fullFolder);

            var storedFileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(fullFolder, storedFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var safeOriginalName = Path.GetFileName(file.FileName);
            var ocrResult = await _ocrService.AnalyzeDocumentAsync(fullPath, file.ContentType, safeOriginalName);

            var doc = new Document
            {
                FileName = safeOriginalName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                Category = category,
                VerificationStatus = DocumentVerificationStatus.Pending,
                OcrExtractedText = ocrResult.ExtractedText,
                OcrQualityNote = ocrResult.QualityNote,
                ApplicationId = applicationId,
                LegalAidRequestId = legalAidRequestId,
                UploadedByUserId = uploadedByUserId,
                UploadedAt = DateTime.UtcNow
            };

            _context.Documents.Add(doc);
            await _context.SaveChangesAsync();

            return (true, "Document uploaded securely and queued for officer verification.", doc);
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Application)
                .Include(d => d.LegalAidRequest)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<(byte[]? Bytes, string? ContentType, string? FileName)> GetFileBytesAsync(Document document)
        {
            var privateRoot = _configuration.GetValue<string>("FileStorage:PrivateUploadRoot") ?? "App_Data/SecureDocuments";
            var fullPath = Path.Combine(_env.ContentRootPath, privateRoot, document.StoredFileName);
            if (!File.Exists(fullPath))
                return (null, null, null);

            var bytes = await File.ReadAllBytesAsync(fullPath);
            return (bytes, document.ContentType, document.FileName);
        }

        public async Task<bool> VerifyDocumentAsync(int documentId, DocumentVerificationStatus status, string? rejectionReason, string officerUserId)
        {
            var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
            if (doc == null) return false;

            doc.VerificationStatus = status;
            doc.RejectionReason = status == DocumentVerificationStatus.Rejected ? rejectionReason : null;
            doc.VerifiedByUserId = officerUserId;
            doc.VerifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var statusText = status == DocumentVerificationStatus.Verified ? "verified" : "rejected";
            var detail = status == DocumentVerificationStatus.Rejected && !string.IsNullOrWhiteSpace(rejectionReason)
                ? $" Reason: {rejectionReason}"
                : string.Empty;

            await _notifications.CreateAsync(
                doc.UploadedByUserId,
                $"Document {status}",
                $"Your document '{doc.FileName}' was {statusText}.{detail}",
                NotificationType.Document,
                doc.ApplicationId.HasValue ? $"/Applications/Details/{doc.ApplicationId}" : "/LegalAid");

            return true;
        }
    }
}
