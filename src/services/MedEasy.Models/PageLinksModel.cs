using Forms;

namespace MedEasy.Models
{
    public class PageLinksModel
    {
        public Link Previous { get; set; }

        public Link Next { get; set; }

        public Link First { get; set; }

        public Link Last { get; set; }
    }
}
