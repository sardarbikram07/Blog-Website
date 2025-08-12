using System.ComponentModel.DataAnnotations;

namespace blogapp.Models
{
    public class Bookmark
    {
        public int Id { get; set; }

        // Foreign Key to User
        public int UserId { get; set; }
        public virtual User User { get; set; }

        // Foreign Key to BlogPost
        public int PostId { get; set; }
        public virtual BlogPost Post { get; set; }

        public DateTime BookmarkedAt { get; set; } = DateTime.Now;
    }
}
