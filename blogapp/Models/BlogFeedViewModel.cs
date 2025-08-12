using System;
using System.Collections.Generic;

namespace blogapp.Models
{
    public class BlogFeedViewModel
    {
        public IEnumerable<BlogPost> Posts { get; set; } = new List<BlogPost>();
        public List<string> AllTags { get; set; } = new List<string>();
        public string Search { get; set; }
        public string SelectedTag { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
