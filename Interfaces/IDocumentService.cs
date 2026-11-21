using Microsoft.AspNetCore.Http;
using NyayaSetu.Models;

namespace NyayaSetu.Interfaces
{
    public interface IDocumentService
    {
        Task<(bool Success, string Message, Document? Document)> UploadAsync(
            IFormFile file,
            DocumentCategory category,
            string uploadedByUserId,
            int? applicationId = null,
            int? legalAidRequestId = null);

        Task<Document?> GetByIdAsync(int id);
        Task<(byte[]? Bytes, string? ContentType, string? FileName)> GetFileBytesAsync(Document document);
        Task<bool> VerifyDocumentAsync(int documentId, DocumentVerificationStatus status, string? rejectionReason, string officerUserId);
    }
}
