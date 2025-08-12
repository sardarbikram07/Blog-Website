using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace blogapp.Models
{
    public class Like
    {
        public int Id { get; set; }

        // Foreign key to BlogPost
        public int BlogPostId { get; set; }

        // Navigation property
        public virtual BlogPost BlogPost { get; set; }

        // Foreign key to User
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.Now;
    }
}
