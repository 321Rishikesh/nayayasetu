using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;

namespace NyayaSetu.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly IDocumentService _documents;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _audit;

        public DocumentsController(
            IDocumentService documents,
            UserManager<ApplicationUser> userManager,
            IAuditService audit)
        {
            _documents = documents;
            _userManager = userManager;
            _audit = audit;
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var doc = await _documents.GetByIdAsync(id);
            if (doc == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            bool isOwner = doc.UploadedByUserId == userId ||
                           doc.Application?.CitizenId == userId ||
                           doc.LegalAidRequest?.CitizenId == userId;
            bool isAuthorizedOfficer = User.IsInRole(AppRoles.SuperAdmin) ||
                                       (User.IsInRole(AppRoles.DepartmentOfficer) && doc.Application?.AssignedOfficerId == userId) ||
                                       (User.IsInRole(AppRoles.LegalAidOfficer) && doc.LegalAidRequestId.HasValue);

            if (!isOwner && !isAuthorizedOfficer)
                return Forbid();

            var (bytes, contentType, fileName) = await _documents.GetFileBytesAsync(doc);
            if (bytes == null) return NotFound("Stored file could not be located on disk.");

            await _audit.LogAsync(userId, User.Identity?.Name, "DownloadDocument", "Document", doc.Id.ToString(), $"Downloaded {doc.FileName}");
            return File(bytes, contentType ?? "application/octet-stream", fileName);
        }
    }
}
