using blogapp.Models;

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public BlogPost Post { get; set; }

    public string Author { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? ParentCommentId { get; set; }
    public Comment ParentComment { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
