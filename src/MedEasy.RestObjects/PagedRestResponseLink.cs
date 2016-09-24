namespace MedEasy.RestObjects
{
    /// <summary>
    /// Holds links in a <see cref="GenericPagedGetResponse{T}"/>
    /// </summary>
    public class PagedRestResponseLink
    {
        /// <summary>
        /// Builds a new <see cref="PagedRestResponseLink"/> instance
        /// </summary>
        /// <param name="first"></param>
        /// <param name="previous"></param>
        /// <param name="next"></param>
        /// <param name="last"></param>
        public PagedRestResponseLink(string first, string previous, string next, string last)
        {
            First = first != null 
                ? new Link { Href = first, Rel = "first" } 
                : null;
            Next = next != null
                ? new Link { Href = next, Rel = "next" }
                : null;
            Last = last != null
                ? new Link { Href = last, Rel = "last" }
                : null;
            ;
            Previous = previous != null
                ? new Link { Href = previous, Rel = "previous" }
                : null;
            ;
        }

        /// <summary>
        /// Link to the first page of result
        /// </summary>
        public Link First { get;  }
        /// <summary>
        /// Link to the next page of result
        /// </summary>
        public Link Next { get; }
        /// <summary>
        /// Link to the last page of result
        /// </summary>
        public Link Last { get; }
        /// <summary>
        /// Link to the previous page of result
        /// </summary>
        public Link Previous { get;}
    }
}

