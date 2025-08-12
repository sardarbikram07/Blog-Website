using blogapp.Data;
using blogapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace blogapp.Controllers
{
    public class AdminNotificationController : Controller
    {
        private readonly BlogDBContext _context;
        private const string AdminPassword = "123456"; // same as used in Admin login

        public AdminNotificationController(BlogDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var notifications = _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
        }

        [HttpPost]
        public IActionResult MarkImportant(int id)
        {
            var notif = _context.Notifications.Find(id);
            if (notif != null)
            {
                notif.IsImportant = !notif.IsImportant;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id, string password)
        {
            var notif = _context.Notifications.Find(id);

            if (notif == null)
                return NotFound();

            if (notif.IsImportant && password != AdminPassword)
            {
                TempData["Error"] = "❌ Incorrect password. Cannot delete important notification.";
                return RedirectToAction("Index");
            }

            _context.Notifications.Remove(notif);
            _context.SaveChanges();

            TempData["Success"] = "✅ Notification deleted.";
            return RedirectToAction("Index");
        }
    }
}
