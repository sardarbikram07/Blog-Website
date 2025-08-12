using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blogapp.Data;
using blogapp.Models;
using System.Linq;

public class FeedController : Controller
{
    private readonly BlogDBContext _context;

    public FeedController(BlogDBContext context)
    {
        _context = context;
    }




    public IActionResult Index(string search = "", string filter = "all")
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var query = _context.BlogPosts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Title != null && EF.Functions.Like(p.Title, $"%{search}%"));
        }

        ViewBag.SelectedFilter = filter;
        switch (filter)
        {
            case "trending":
                // Trending: Posts with likes in the last 7 days
                var oneWeekAgo = DateTime.Now.AddDays(-7);
                query = query.Where(p => p.Likes.Any(l => l.LikedAt >= oneWeekAgo))
                            .OrderByDescending(p => p.Likes.Count(l => l.LikedAt >= oneWeekAgo));
                break;
            case "latest":
                // Latest: Most recent posts
                query = query.OrderByDescending(p => p.CreatedAt);
                break;
            case "popular":
                // Popular: Posts with most likes overall
                query = query.OrderByDescending(p => p.Likes.Count);
                break;
            default:
                // All Posts: Default order by creation date
                query = query.OrderByDescending(p => p.CreatedAt);
                break;
        }

        var posts = query.Take(10).ToList();

        ViewBag.UserId = userId;
        ViewBag.SearchQuery = search;

        var bookmarkedIds = _context.Bookmarks
            .Where(b => b.UserId == userId)
            .Select(b => b.PostId)
            .ToList();

        ViewBag.BookmarkedIds = bookmarkedIds;

        return View(posts);
    }



    [HttpGet]
    public IActionResult LoadMore(int skip, string search = "", string filter = "all")
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var query = _context.BlogPosts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Title.Contains(search));

        switch (filter)
        {
            case "trending":
                var oneWeekAgo = DateTime.Now.AddDays(-7);
                query = query.Where(p => p.Likes.Any(l => l.LikedAt >= oneWeekAgo))
                            .OrderByDescending(p => p.Likes.Count(l => l.LikedAt >= oneWeekAgo));
                break;
            case "latest":
                query = query.OrderByDescending(p => p.CreatedAt);
                break;
            case "popular":
                query = query.OrderByDescending(p => p.Likes.Count);
                break;
            default:
                query = query.OrderByDescending(p => p.CreatedAt);
                break;
        }

        var posts = query.Skip(skip).Take(5).ToList();

        ViewBag.UserId = userId;
        ViewBag.BookmarkedIds = _context.Bookmarks
            .Where(b => b.UserId == userId)
            .Select(b => b.PostId)
            .ToList();

        return PartialView("_BlogPostPartial", posts);
    }

    [HttpGet]
    public IActionResult FilterPosts(string filter = "all", string search = "")
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var query = _context.BlogPosts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Title != null && EF.Functions.Like(p.Title, $"%{search}%"));
        }

        switch (filter)
        {
            case "trending":
                var oneWeekAgo = DateTime.Now.AddDays(-7);
                query = query.Where(p => p.Likes.Any(l => l.LikedAt >= oneWeekAgo))
                            .OrderByDescending(p => p.Likes.Count(l => l.LikedAt >= oneWeekAgo));
                break;
            case "latest":
                query = query.OrderByDescending(p => p.CreatedAt);
                break;
            case "popular":
                query = query.OrderByDescending(p => p.Likes.Count);
                break;
            default:
                query = query.OrderByDescending(p => p.CreatedAt);
                break;
        }

        var posts = query.Take(10).ToList();

        ViewBag.UserId = userId;
        ViewBag.BookmarkedIds = _context.Bookmarks
            .Where(b => b.UserId == userId)
            .Select(b => b.PostId)
            .ToList();

        return PartialView("_BlogPostPartialList", posts);
    }
   


}
