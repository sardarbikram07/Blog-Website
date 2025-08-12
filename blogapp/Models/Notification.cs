// Models/Notification.cs
using System;
using System.Collections.Generic;

namespace blogapp.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }  // e.g. Blog Approved, New Post
        public string Message { get; set; } // Full message
        public DateTime CreatedAt { get; set; }
        public bool IsImportant { get; set; } // NEW
        public bool IsForCreators { get; set; } // if true, only for blog creators
        public bool IsRead { get; set; }
        
        // Navigation property
        public ICollection<UserNotification> UserNotifications { get; set; }
    }
}
