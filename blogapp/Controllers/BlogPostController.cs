using blogapp.Data;
using blogapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic; // Added for List

namespace blogapp.Controllers
{
    public class BlogPostController : Controller
    {
        private readonly BlogDBContext _context;
        private readonly IWebHostEnvironment _env;

        public BlogPostController(BlogDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        //  Display all blog posts
        [AllowAnonymous]
        public IActionResult Index()
        {
            var posts = _context.BlogPosts
                .AsNoTracking()
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToList();

            var userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.UserId = userId;

            if (userId != null)
            {
                var bookmarkedIds = _context.Bookmarks
                    .Where(b => b.UserId == userId.Value)
                    .Select(b => b.PostId)
                    .ToList();

                ViewBag.BookmarkedIds = bookmarkedIds;
            }

            return View(posts);
        }

        // View full post details
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            try
            {
                Console.WriteLine($"Details action called for post ID: {id}");
                
                var post = _context.BlogPosts
                    .Include(p => p.Comments).ThenInclude(c => c.Replies)
                    .Include(p => p.Likes)
                    .Include(p => p.User)
                    .FirstOrDefault(p => p.Id == id);

                if (post == null)
                {
                    Console.WriteLine($"Post with ID {id} not found");
                    return NotFound();
                }

                Console.WriteLine($"Post found: {post.Title} by {post.User?.Name}");
                
                var userId = HttpContext.Session.GetInt32("UserId");
                ViewBag.AlreadyLiked = userId != null &&
                    _context.Likes.Any(l => l.BlogPostId == id && l.UserId == userId);

                return View(post);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Details action: {ex.Message}");
                return NotFound();
            }
        }

        // Clean, minimal view for shared links
        [AllowAnonymous]
        public IActionResult SharedView(int id)
        {
            try
            {
                Console.WriteLine($"SharedView action called for post ID: {id}");
                
                var post = _context.BlogPosts
                    .Include(p => p.User)
                    .FirstOrDefault(p => p.Id == id);

                if (post == null)
                {
                    Console.WriteLine($"Post with ID {id} not found");
                    return NotFound();
                }

                Console.WriteLine($"Shared post found: {post.Title} by {post.User?.Name}");
                
                return View(post);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SharedView action: {ex.Message}");
                return NotFound();
            }
        }

        // Feed blog details view with full functionality
        public IActionResult FeedDetails(int id)
        {
            try
            {
                Console.WriteLine($"FeedDetails action called for post ID: {id}");
                
                var post = _context.BlogPosts
                    .Include(p => p.Comments).ThenInclude(c => c.Replies)
                    .Include(p => p.Likes)
                    .Include(p => p.User)
                    .FirstOrDefault(p => p.Id == id);

                if (post == null)
                {
                    Console.WriteLine($"Post with ID {id} not found");
                    return NotFound();
                }

                Console.WriteLine($"Feed post found: {post.Title} by {post.User?.Name}");
                
                var userId = HttpContext.Session.GetInt32("UserId");
                ViewBag.AlreadyLiked = userId != null &&
                    _context.Likes.Any(l => l.BlogPostId == id && l.UserId == userId);

                return View(post);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FeedDetails action: {ex.Message}");
                return NotFound();
            }
        }

        //  GET: Create blog post form
        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }
            // Only allow approved users to create posts
            if (user.BlogAccessStatus != AccessStatus.Approved)
            {
                TempData["Message"] = "Your account is pending admin approval. You cannot post blogs until approved.";
                return RedirectToAction("Dashboard", "User");
            }
            return View();
        }


        //  POST: Create a new blog post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BlogPost blogPost)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Auth");
            }
            // Only allow approved users to post
            if (user.BlogAccessStatus != AccessStatus.Approved)
            {
                TempData["Message"] = "Your account is pending admin approval. You cannot post blogs until approved.";
                return RedirectToAction("Dashboard", "User");
            }
            if (!ModelState.IsValid)
                return View(blogPost);

            // Save image if uploaded
            if (blogPost.imageFile != null && blogPost.imageFile.Length > 0)
            {
                var imageFileName = Guid.NewGuid() + Path.GetExtension(blogPost.imageFile.FileName);
                var imagePath = Path.Combine("wwwroot/uploads", imageFileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    blogPost.imageFile.CopyTo(stream);
                }
                blogPost.ImagePath = "/uploads/" + imageFileName;
            }

            // Save video if uploaded
            if (blogPost.videoFile != null && blogPost.videoFile.Length > 0)
            {
                var videoFileName = Guid.NewGuid() + Path.GetExtension(blogPost.videoFile.FileName);
                var videoPath = Path.Combine("wwwroot/uploads", videoFileName);
                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    blogPost.videoFile.CopyTo(stream);
                }
                blogPost.VideoPath = "/uploads/" + videoFileName;
            }

            blogPost.CreatedAt = DateTime.Now;
            blogPost.UserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            _context.BlogPosts.Add(blogPost);
            _context.SaveChanges();

            return RedirectToAction("Index", "Feed");
        }





        //  Debug action to check likes
        [HttpGet]
        public IActionResult DebugLikes(int postId)
        {
            var likes = _context.Likes.Where(l => l.BlogPostId == postId).ToList();
            var likeCount = likes.Count;
            var likeDetails = likes.Select(l => new { UserId = l.UserId, LikedAt = l.LikedAt }).ToList();
            
            return Json(new { 
                PostId = postId, 
                LikeCount = likeCount, 
                LikeDetails = likeDetails 
            });
        }

        //  AJAX Like a blog post (returns JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LikeAjax(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return Json(new { success = false });

                // Check if post exists
                var post = _context.BlogPosts.FirstOrDefault(p => p.Id == id);
                if (post == null)
                    return Json(new { success = false });

                var existingLike = _context.Likes.FirstOrDefault(l => l.BlogPostId == id && l.UserId == userId);
                var isLiked = false;
                
                if (existingLike == null)
                {
                    // Add like
                    var like = new Like
                    {
                        BlogPostId = id,
                        UserId = userId.Value,
                        LikedAt = DateTime.Now
                    };

                    _context.Likes.Add(like);
                    isLiked = true;

                    // Create personal notification for the post author (if not liking their own post)
                    if (post.UserId != userId.Value)
                    {
                        var liker = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
                        var notification = new Notification
                        {
                            Title = "New Like",
                            Message = $"{liker?.Name ?? "Someone"} liked your post: \"{post.Title}\"",
                            CreatedAt = DateTime.Now,
                            IsImportant = false,
                            IsForCreators = false,
                            IsRead = false
                        };

                        _context.Notifications.Add(notification);
                        _context.SaveChanges();

                        // Create UserNotification entry for the post author
                        var userNotification = new UserNotification
                        {
                            UserId = post.UserId,
                            NotificationId = notification.Id,
                            IsRead = false,
                            ReadAt = DateTime.MinValue
                        };

                        _context.UserNotifications.Add(userNotification);
                    }
                }
                else
                {
                    // Remove like (unlike)
                    _context.Likes.Remove(existingLike);
                    isLiked = false;
                }

                _context.SaveChanges();

                // Get updated like count
                var likeCount = _context.Likes.Count(l => l.BlogPostId == id);

                return Json(new { 
                    success = true, 
                    isLiked = isLiked, 
                    likeCount = likeCount
                });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"LikeAjax Error: {ex.Message}");
                return Json(new { success = false });
            }
        }

        //  Like a blog post
        [HttpPost]
        public IActionResult Like(int id, string returnUrl = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var existingLike = _context.Likes.FirstOrDefault(l => l.BlogPostId == id && l.UserId == userId);
            
            if (existingLike == null)
            {
                // Add like
                var like = new Like
                {
                    BlogPostId = id,
                    UserId = userId.Value,
                    LikedAt = DateTime.Now
                };

                _context.Likes.Add(like);
                _context.SaveChanges();
                
                // Add success message
                TempData["LikeMessage"] = "Post liked successfully!";
            }
            else
            {
                // Remove like (unlike)
                _context.Likes.Remove(existingLike);
                _context.SaveChanges();
                
                // Add success message
                TempData["LikeMessage"] = "Post unliked successfully!";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Details", new { id });
        }

        //  Delete a blog post
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var post = _context.BlogPosts
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .Include(p => p.Bookmarks)
                .FirstOrDefault(p => p.Id == id);

            if (post != null)
            {
                // Delete related comments
                _context.Comments.RemoveRange(post.Comments);

                // Delete related bookmarks
                _context.Bookmarks.RemoveRange(post.Bookmarks);

                // Likes will be deleted automatically due to cascade

                // Delete image file if exists
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    string fullPath = Path.Combine(_env.WebRootPath, post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                _context.BlogPosts.Remove(post);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Feed");
        }

        //  Debug Comments for a specific post
        [HttpGet]
        public IActionResult DebugComments(int postId)
        {
            var comments = _context.Comments
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            var commentDetails = comments.Select(c => new
            {
                Id = c.Id,
                PostId = c.PostId,
                Author = c.Author,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                ParentCommentId = c.ParentCommentId
            }).ToList();

            return Json(new
            {
                PostId = postId,
                CommentCount = comments.Count,
                Comments = commentDetails
            });
        }

        //  Add a comment or reply
        [HttpPost]
        public IActionResult AddComment(int postId, string content, int? parentCommentId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            
            if (userId == null || string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get the post to check if user is commenting on their own post
            var post = _context.BlogPosts.FirstOrDefault(p => p.Id == postId);
            if (post == null)
                return RedirectToAction("Index", "Feed");

            var comment = new Comment
            {
                PostId = postId,
                Author = userName,
                Content = content,
                CreatedAt = DateTime.Now,
                ParentCommentId = parentCommentId
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            // Create personal notification for the post author (if not commenting on their own post)
            if (post.UserId != userId.Value)
            {
                var notification = new Notification
                {
                    Title = "New Comment",
                    Message = $"{userName} commented on your post: \"{post.Title}\"",
                    CreatedAt = DateTime.Now,
                    IsImportant = false,
                    IsForCreators = false,
                    IsRead = false
                };

                _context.Notifications.Add(notification);
                _context.SaveChanges();

                // Create UserNotification entry for the post author
                var userNotification = new UserNotification
                {
                    UserId = post.UserId,
                    NotificationId = notification.Id,
                    IsRead = false,
                    ReadAt = DateTime.MinValue
                };

                _context.UserNotifications.Add(userNotification);
                _context.SaveChanges();
            }

            // Add success message
            TempData["CommentMessage"] = "Comment posted successfully!";

            return RedirectToAction("Details", new { id = postId });
        }

        //  GET: Edit post form
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            // Check if user is logged in
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var post = _context.BlogPosts.Find(id);
            if (post == null)
                return NotFound();

            // Check if user owns this post or has admin access
            if (post.UserId != userId)
            {
                // Check if user is admin
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null || user.BlogAccessStatus != AccessStatus.Approved)
                {
                    return RedirectToAction("Index", "Feed");
                }
            }

            return View(post);
        }

        // POST: Save edited post
        [HttpPost]
        public IActionResult Edit(BlogPost updatedPost, IFormFile imageFile, IFormFile videoFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            // Check if user is logged in
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var post = _context.BlogPosts.Find(updatedPost.Id);
            if (post == null)
                return NotFound();

            // Check if user owns this post or has admin access
            if (post.UserId != userId)
            {
                // Check if user is admin
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null || user.BlogAccessStatus != AccessStatus.Approved)
                {
                    return RedirectToAction("Index", "Feed");
                }
            }

            // ✏️ Update fields
            post.Title = updatedPost.Title;
            post.Content = updatedPost.Content;
            post.Tags = updatedPost.Tags;
            post.Category = updatedPost.Category; // ✅ Category update

            // 📷 Handle new image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, post.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                string imageFileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                string imagePath = Path.Combine(uploadsFolder, imageFileName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                post.ImagePath = "/uploads/" + imageFileName;
            }

            // 🎥 Handle new video upload
            if (videoFile != null && videoFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(post.VideoPath))
                {
                    var oldVideoPath = Path.Combine(_env.WebRootPath, post.VideoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldVideoPath))
                        System.IO.File.Delete(oldVideoPath);
                }

                string videoFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(videoFolder);

                string videoFileName = Guid.NewGuid() + Path.GetExtension(videoFile.FileName);
                string videoPath = Path.Combine(videoFolder, videoFileName);

                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    videoFile.CopyTo(stream);
                }

                post.VideoPath = "/uploads/" + videoFileName;
            }

            _context.SaveChanges();
            return RedirectToAction("Index", "Feed");
        }
        public IActionResult Feed(string search, string sort)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var posts = _context.BlogPosts
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .AsQueryable();

            // 🔍 Apply search
            if (!string.IsNullOrWhiteSpace(search))
            {
                posts = posts.Where(p => p.Title.Contains(search) || p.Content.Contains(search) || p.Tags.Contains(search));
            }

            // ↕️ Apply sorting
            switch (sort)
            {
                case "newest":
                    posts = posts.OrderByDescending(p => p.CreatedAt);
                    break;
                case "likes":
                    posts = posts.OrderByDescending(p => p.Likes.Count);
                    break;
                case "comments":
                    posts = posts.OrderByDescending(p => p.Comments.Count);
                    break;
                case "editor":
                    posts = posts.Where(p => p.IsAdminChoice).OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    posts = posts.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            // 📌 Bookmarks
            var bookmarkedIds = new List<int>();
            if (userId != null)
            {
                bookmarkedIds = _context.Bookmarks
                    .Where(b => b.UserId == userId)
                    .Select(b => b.PostId)

                    .ToList();
            }

            ViewBag.UserId = userId;
            ViewBag.BookmarkedIds = bookmarkedIds;
            ViewBag.SelectedSort = sort;
            ViewBag.SearchQuery = search;

            return View(posts
                .AsNoTracking() // ✅ Reduces memory usage
                .Take(50)       // ✅ Load only top 50 results
                .ToList());

        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Reader()
        {
            try
            {
                // Ensure we have at least one user
                var defaultUser = _context.Users.FirstOrDefault();

                // Only add a blog if there's a user to own it
                if (!_context.BlogPosts.Any() && defaultUser != null)
                {
                    _context.BlogPosts.Add(new BlogPost
                    {
                        Title = "Welcome to BlogHub!",
                        Content = "This is a sample blog post content.",
                        CreatedAt = DateTime.Now,
                        UserId = defaultUser.Id
                    });

                    _context.SaveChanges();
                }

                var posts = _context.BlogPosts
                    .Include(p => p.User)
                    .Include(p => p.Comments)
                    .Where(p => !string.IsNullOrWhiteSpace(p.Content))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(10)
                    .ToList();

                return View(posts);
            }
            catch (Exception ex)
            {
                // Optional: log ex.Message here
                return View("Error");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult TrendingPage()
        {
            var allTime = _context.BlogPosts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Where(p => p.Likes.Count > 0)
                .OrderByDescending(p => p.Likes.Count)
                .Take(5)
                .ToList();

            var oneWeekAgo = DateTime.Now.AddDays(-7);

            var weekly = _context.BlogPosts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Where(p => p.Likes.Any(l => l.LikedAt >= oneWeekAgo))
                .GroupBy(p => p.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    TopPost = g.OrderByDescending(p => p.Likes.Count(l => l.LikedAt >= oneWeekAgo)).FirstOrDefault()
                })
                .ToList()
                .Select(x => (x.Category, x.TopPost))
                .ToList();

            return View("TrendingPage", (allTime, weekly));
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddPublicComment(int postId, string author, string content)
        {
            var comment = new Comment
            {
                PostId = postId,
                Author = author,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            return RedirectToAction("Reader");
        }





    }
}
