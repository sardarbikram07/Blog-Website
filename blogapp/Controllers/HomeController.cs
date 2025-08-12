using blogapp.Data;
using blogapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace blogapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BlogDBContext _context;

        public HomeController(ILogger<HomeController> logger, BlogDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Public page for anyone
        public IActionResult Landing()
        {
            var posts = _context.BlogPosts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToList();
            return View(posts); // Pass recent posts to the view
        }

        


        [HttpGet]
        public IActionResult Index(string search, string sort)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var posts = _context.BlogPosts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Take(50)
                .ToList();

            if (!string.IsNullOrEmpty(search))
            {
                posts = posts
                    .Where(p => p.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                ViewBag.SearchQuery = search;
            }

            ViewBag.SelectedSort = sort;
            posts = sort switch
            {
                "newest" => posts.OrderByDescending(p => p.CreatedAt).ToList(),
                "likes" => posts.OrderByDescending(p => p.Likes.Count).ToList(),
                "comments" => posts.OrderByDescending(p => p.Comments.Count).ToList(),
                "editor" => posts.OrderByDescending(p => p.IsAdminChoice).ToList(),
                _ => posts
            };

            ViewBag.UserId = userId;
            ViewBag.BookmarkedIds = _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Select(b => b.PostId)
                .ToList();

            return View(posts);
        }






        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
