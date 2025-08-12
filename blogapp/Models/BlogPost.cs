using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace blogapp.Models
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        public int ViewCount { get; set; } = 0;


        [Required]
        public string Content { get; set; } = string.Empty;

        public string? Tags { get; set; }
       
        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsAdminChoice { get; set; } = false;
        public int Views { get; set; } = 0;

        //public int UserId { get; set; }
        //public virtual User User { get; set; }
       
        //public string? VideoPath { get; set; }
        public string? Category { get; set; }
        public bool IsApproved { get; set; } = false;
        public int UserId { get; set; }     // This is the only required field
        public User? User { get; set; }
        // No [Required] here
        [NotMapped]
        public IFormFile? imageFile { get; set; }

        [NotMapped]
        public IFormFile? videoFile { get; set; }

        public string? VideoPath { get; set; }   // Make it nullable if optional

        // Navigation Properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    }
}
