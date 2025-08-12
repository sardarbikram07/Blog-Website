using blogapp.Models;

public class DashboardViewModel
{
    public int TotalPosts { get; set; }
    public int TotalComments { get; set; }
    public int TotalLikes { get; set; }
    public List<BlogPost> RecentPosts { get; set; } = new();
    public List<BlogPost> BookmarkedPosts { get; set; } = new List<BlogPost>();

}
