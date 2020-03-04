using System;
using System.Collections.Generic;

namespace hatenagroup
{
    public class Entry
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public IList<string> Tags { get; set; }
        public string Body { get; set; }
        public IList<Comment> Comments { get; set; }
    }
}