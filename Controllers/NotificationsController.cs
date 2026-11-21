using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;

namespace NyayaSetu.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notifications;

        public NotificationsController(UserManager<ApplicationUser> userManager, INotificationService notifications)
        {
            _userManager = userManager;
            _notifications = notifications;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var list = await _notifications.GetUserNotificationsAsync(userId, 50);
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = _userManager.GetUserId(User)!;
            var count = await _notifications.GetUnreadCountAsync(userId);
            return Json(new { count });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            await _notifications.MarkAsReadAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = _userManager.GetUserId(User)!;
            await _notifications.MarkAllAsReadAsync(userId);
            TempData["Success"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Index));
        }
    }
}
