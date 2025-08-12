using blogapp.Data;
using blogapp.Models;
using blogapp.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace blogapp.Controllers
{
    public class UserController : Controller
    {
        private readonly BlogDBContext _context;
        private readonly IWebHostEnvironment _env;

        public UserController(BlogDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        //  GET: /User/Profile
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                ProfileImagePath = user.ProfileImagePath
            };

            return View(viewModel);
        }

        //  POST: Update Profile
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult UpdateProfile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Id == model.Id);
            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Email = model.Email;

            // If user entered a new password, update it
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            _context.SaveChanges();

            // Clear session and redirect to Login page
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }


        //  GET: /User/MyPosts
        public IActionResult MyPosts()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var posts = _context.BlogPosts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            var bookmarkedIds = _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Select(b => b.PostId)
                .ToList();

            ViewBag.BookmarkedIds = bookmarkedIds;

            return View(posts);
        }

        //  POST: Delete a post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var post = _context.BlogPosts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .Include(p => p.Bookmarks)
                .FirstOrDefault(p => p.Id == id && p.UserId == userId);
                
            if (post != null)
            {
                // Remove all related entities first
                _context.Comments.RemoveRange(post.Comments);
                _context.Likes.RemoveRange(post.Likes);
                _context.Bookmarks.RemoveRange(post.Bookmarks);
                
                // Remove image file if exists
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                // Remove video file if exists
                if (!string.IsNullOrEmpty(post.VideoPath))
                {
                    var videoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.VideoPath.TrimStart('/'));
                    if (System.IO.File.Exists(videoPath))
                        System.IO.File.Delete(videoPath);
                }

                _context.BlogPosts.Remove(post);
                _context.SaveChanges();
            }

            return RedirectToAction("MyPosts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAjax(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { success = false, message = "Please login first." });

            var post = _context.BlogPosts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .Include(p => p.Bookmarks)
                .FirstOrDefault(p => p.Id == id && p.UserId == userId);
                
            if (post == null) return Json(new { success = false, message = "Post not found." });

            try
            {
                // Remove all related entities first
                _context.Comments.RemoveRange(post.Comments);
                _context.Likes.RemoveRange(post.Likes);
                _context.Bookmarks.RemoveRange(post.Bookmarks);
                
                // Remove image file if exists
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                // Remove video file if exists
                if (!string.IsNullOrEmpty(post.VideoPath))
                {
                    var videoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.VideoPath.TrimStart('/'));
                    if (System.IO.File.Exists(videoPath))
                        System.IO.File.Delete(videoPath);
                }

                _context.BlogPosts.Remove(post);
                _context.SaveChanges();

                return Json(new { success = true, message = "Post deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting post: " + ex.Message });
            }
        }

        // ✏️ GET: Edit post
        [HttpGet]
        [RequestSizeLimit(209715200)] // 200 MB
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var post = _context.BlogPosts.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (post == null) return NotFound();

            return View("~/Views/User/Edit.cshtml", post); // You can rename Edit1.cshtml to Edit.cshtml later
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BlogPost updatedPost, IFormFile imageFile, IFormFile videoFile)
        {
            var post = _context.BlogPosts.Find(updatedPost.Id);
            if (post == null)
                return NotFound();

            post.Title = updatedPost.Title;
            post.Content = updatedPost.Content;
            post.Tags = updatedPost.Tags;
            post.Category = updatedPost.Category;

            // Save new image
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var newName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                using var stream = new FileStream(Path.Combine(uploads, newName), FileMode.Create);
                imageFile.CopyTo(stream);

                post.ImagePath = "/uploads/" + newName;
            }

            // Save new video
            if (videoFile != null && videoFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                if (!string.IsNullOrEmpty(post.VideoPath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, post.VideoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var newName = Guid.NewGuid() + Path.GetExtension(videoFile.FileName);
                using var stream = new FileStream(Path.Combine(uploads, newName), FileMode.Create);
                videoFile.CopyTo(stream);

                post.VideoPath = "/uploads/" + newName;
            }

            _context.SaveChanges();

            TempData["Message"] = "✅ Blog updated successfully!";
            return RedirectToAction("Index", "Feed");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Edit(BlogPost updatedPost, IFormFile imageFile, IFormFile videoFile)
        //{
        //    var post = _context.BlogPosts.Find(updatedPost.Id);
        //    if (post == null)
        //        return NotFound();

        //    post.Title = updatedPost.Title;
        //    post.Content = updatedPost.Content;
        //    post.Tags = updatedPost.Tags;
        //    post.Category = updatedPost.Category;

        //    // Save new image
        //    if (imageFile != null && imageFile.Length > 0)
        //    {
        //        var uploads = Path.Combine(_env.WebRootPath, "uploads");
        //        Directory.CreateDirectory(uploads);

        //        if (!string.IsNullOrEmpty(post.ImagePath))
        //        {
        //            var oldPath = Path.Combine(_env.WebRootPath, post.ImagePath.TrimStart('/'));
        //            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        //        }

        //        var newName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
        //        using var stream = new FileStream(Path.Combine(uploads, newName), FileMode.Create);
        //        imageFile.CopyTo(stream);

        //        post.ImagePath = "/uploads/" + newName;
        //    }

        //    // Validate video size before saving
        //    const long maxVideoSize = 200 * 1024 * 1024; // 200 MB
        //    if (videoFile != null && videoFile.Length > 0)
        //    {
        //        if (videoFile.Length > maxVideoSize)
        //        {
        //            ModelState.AddModelError("VideoFile", "❌ The uploaded video exceeds the 100 MB limit.");
        //            return View("~/Views/User/Edit.cshtml", post);
        //        }

        //        var uploads = Path.Combine(_env.WebRootPath, "uploads");
        //        Directory.CreateDirectory(uploads);

        //        if (!string.IsNullOrEmpty(post.VideoPath))
        //        {
        //            var oldPath = Path.Combine(_env.WebRootPath, post.VideoPath.TrimStart('/'));
        //            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        //        }

        //        var newName = Guid.NewGuid() + Path.GetExtension(videoFile.FileName);
        //        using var stream = new FileStream(Path.Combine(uploads, newName), FileMode.Create);
        //        videoFile.CopyTo(stream);

        //        post.VideoPath = "/uploads/" + newName;
        //    }

        //    _context.SaveChanges();

        //    TempData["Message"] = "✅ Blog updated successfully!";
        //    return RedirectToAction("Index", "Feed");
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleBookmark(int id, string returnUrl = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var post = _context.BlogPosts.FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                TempData["Error"] = "Post not found.";
                return RedirectToAction("Index", "Feed");
            }

            var bookmark = _context.Bookmarks.FirstOrDefault(b => b.UserId == userId && b.PostId == id);

            if (bookmark != null)
            {
                _context.Bookmarks.Remove(bookmark);
                TempData["BookmarkMessage"] = "Bookmark removed.";
            }
            else
            {
                _context.Bookmarks.Add(new Bookmark
                {
                    UserId = userId.Value,
                    PostId = id,
                    BookmarkedAt = DateTime.Now
                });
                TempData["BookmarkMessage"] = "Post bookmarked!";
            }

            _context.SaveChanges();

            // Safely redirect back
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            string referer = Request.Headers["Referer"].ToString();
            return Redirect(referer);

            //return RedirectToAction("Dashboard", "User");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleBookmarkAjax(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Json(new { success = false, message = "Please login first." });

            var post = _context.BlogPosts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return Json(new { success = false, message = "Post not found." });

            var bookmark = _context.Bookmarks.FirstOrDefault(b => b.UserId == userId && b.PostId == id);
            bool isBookmarked = false;

            if (bookmark != null)
            {
                _context.Bookmarks.Remove(bookmark);
                isBookmarked = false;
            }
            else
            {
                _context.Bookmarks.Add(new Bookmark
                {
                    UserId = userId.Value,
                    PostId = id,
                    BookmarkedAt = DateTime.Now
                });
                isBookmarked = true;
            }

            _context.SaveChanges();

            return Json(new { 
                success = true, 
                isBookmarked = isBookmarked,
                message = "" // Removed success message
            });
        }


        //  GET: Bookmarked Posts
        public IActionResult Bookmarks()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var posts = _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Post)
                    .ThenInclude(p => p.Likes)
                .Include(b => b.Post)
                    .ThenInclude(p => p.Comments)
                .Select(b => b.Post)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            ViewBag.UserId = userId;
            return View("Bookmarks", posts);
        }

        //  GET: Dashboard
        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var model = new DashboardViewModel
            {
                TotalPosts = _context.BlogPosts.Count(p => p.UserId == userId),
                TotalComments = _context.Comments.Count(c => c.Post.UserId == userId),
                TotalLikes = _context.Likes.Count(l => l.BlogPost.UserId == userId),
                RecentPosts = _context.BlogPosts
                    .Where(p => p.UserId == userId)
                    .Include(p => p.Likes)
                    .Include(p => p.Comments)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToList(),
                BookmarkedPosts = _context.Bookmarks
                    .Where(b => b.UserId == userId)
                    .Include(b => b.Post)
                        .ThenInclude(p => p.Likes)
                    .Include(b => b.Post)
                        .ThenInclude(p => p.Comments)
                    .Select(b => b.Post)
                    .ToList()
            };

            return View(model);
        }

        //  Password hashing (replace with BCrypt in production)
        private string HashPassword(string password)
        {
            return password;
        }

        /// Clean TinyMCE content
        private string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;
            return Regex.Replace(html, @"\sdata-[\w-]+=""[^""]*""", "", RegexOptions.IgnoreCase);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || imageFile == null || imageFile.Length == 0)
            {
                TempData["Error"] = "Invalid upload.";
                return RedirectToAction("Dashboard");
            }

            // Save the image to wwwroot/images/profiles/
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"user_{userId}_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            //  path
            var relativePath = "/images/profiles/" + uniqueFileName;

            // Update user
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.ProfileImagePath = relativePath;
                _context.SaveChanges();
                HttpContext.Session.SetString("ProfileImagePath", relativePath);
            }

            TempData["Message"] = "Profile uploaded successfully!";
            return RedirectToAction("Dashboard");
        }

        //become creator

        [HttpGet]
        public IActionResult BecomeCreator()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BecomeCreatorRequest()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            // Check if user is logged in
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            
            // Check if user exists
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }

            // Check if user can request blog access
            if (user.BlogAccessStatus == AccessStatus.None)
            {
                user.BlogAccessStatus = AccessStatus.Pending;
                _context.SaveChanges();

                TempData["RequestMessage"] = "Your blog creator request has been submitted successfully! Please wait for admin approval.";
                return RedirectToAction("BecomeCreator", "User");
            }
            else if (user.BlogAccessStatus == AccessStatus.Pending)
            {
                TempData["RequestMessage"] = "You already have a pending request. Please wait for admin approval.";
                return RedirectToAction("BecomeCreator", "User");
            }
            else if (user.BlogAccessStatus == AccessStatus.Approved)
            {
                TempData["RequestMessage"] = "You already have blog creator access!";
                return RedirectToAction("BecomeCreator", "User");
            }
            else if (user.BlogAccessStatus == AccessStatus.Rejected)
            {
                TempData["RequestMessage"] = "Your previous request was rejected. Please contact admin for more information.";
                return RedirectToAction("BecomeCreator", "User");
            }

            return RedirectToAction("BecomeCreator", "User");
        }


        // You can also add EditProfile if you're linking to it
        // GET: /User/EditProfile - Edit user profile
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                ProfileImagePath = user.ProfileImagePath
            };

            return View("Profile", viewModel); // Use the existing Profile view
        }

        //  Debug Bookmarks for a specific user
        [HttpGet]
        public IActionResult DebugBookmarks(int userId)
        {
            var bookmarks = _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Post)
                    .ThenInclude(p => p.Likes)
                .Include(b => b.Post)
                    .ThenInclude(p => p.Comments)
                .OrderByDescending(b => b.BookmarkedAt)
                .ToList();

            var bookmarkDetails = bookmarks.Select(b => new
            {
                Id = b.Id,
                UserId = b.UserId,
                PostId = b.PostId,
                PostTitle = b.Post?.Title,
                LikeCount = b.Post?.Likes?.Count ?? 0,
                CommentCount = b.Post?.Comments?.Count ?? 0,
                BookmarkedAt = b.BookmarkedAt
            }).ToList();

            return Json(new
            {
                UserId = userId,
                BookmarkCount = bookmarks.Count,
                Bookmarks = bookmarkDetails
            });
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                var userNotification = await _context.UserNotifications
                    .FirstOrDefaultAsync(un => un.UserId == userId.Value && un.NotificationId == notificationId);

                if (userNotification != null)
                {
                    userNotification.IsRead = true;
                    userNotification.ReadAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error marking notification as read" });
            }
        }

    }
}


