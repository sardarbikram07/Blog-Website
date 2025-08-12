

using blogapp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace blogapp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly BlogDBContext _context;
        private static readonly string[] ValidCategories = {
            "Sports", "Movies", "Cooking", "Technology",
            "Education", "Travel", "Fashion", "News",
            "Wildlife", "Opinion", "Dailylife", "Other"
        };

        public CategoryController(BlogDBContext context)
        {
            _context = context;
        }

        [HttpGet("Category/{name}")]
        public IActionResult Index(string name)
        {
            if (string.IsNullOrEmpty(name) || !ValidCategories.Contains(name))
                return NotFound();

            var posts = _context.BlogPosts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.Category == name)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            if (!posts.Any())
            {
                ViewBag.EmptyMessage = $"No posts found in {name} category";
            }

            ViewBag.Category = name;
            ViewBag.CategoryIcon = GetIconForCategory(name);

            return View(name, posts); // Views/Category/{name}.cshtml
        }

        private string GetIconForCategory(string category)
        {
            return category switch
            {
                "Sports" => "fa-running",
                "Movies" => "fa-film",
                "Cooking" => "fa-utensils",
                "Technology" => "fa-laptop-code",
                "Education" => "fa-graduation-cap",
                "Travel" => "fa-plane",
                "Fashion" => "fa-tshirt",
                "News" => "fa-newspaper",
                "Wildlife" => "fa-paw",
                "Opinion" => "fa-comment-dots",
                "Dailylife" => "fa-calendar-day",
                "Other" => "fa-ellipsis-h",
                _ => "fa-folder"
            };
        }
        [HttpGet("Category")]
        public IActionResult Index()
        {
            return View(); // returns the general category list
        }

    }
}
