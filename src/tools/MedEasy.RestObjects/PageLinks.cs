using Forms;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// Holds links in a <see cref="GenericPagedGetResponse{T}"/>
    /// </summary>
    public struct PageLinks
    {
        /// <summary>
        /// Builds a new <see cref="PageLinks"/> instance
        /// </summary>
        /// <param name="first">link to the first page</param>
        /// <param name="previous">link to the next page</param>
        /// <param name="next">link to the previous page</param>
        /// <param name="last">link to the last page</param>
        public PageLinks(string first, string previous, string next, string last)
        {
            First = first != null
                ? new Link { Href = first, Relation = LinkRelation.First, Method = "GET" }
                : null;
            Next = next != null
                ? new Link { Href = next, Relation = LinkRelation.Next, Method = "GET" }
                : null;
            Last = last != null
                ? new Link { Href = last, Relation = LinkRelation.Last, Method = "GET" }
                : null;
            Previous = previous != null
                ? new Link { Href = previous, Relation = LinkRelation.Previous, Method = "GET" }
                : null;
        }

        /// <summary>
        /// <see cref="Link"/> to the first page of result
        /// </summary>
        public Link First { get;  }

        /// <summary>
        /// <see cref="Link"/> to the next page of result
        /// </summary>
        public Link Next { get; }

        /// <summary>
        /// <see cref="Link"/> to the last page of result
        /// </summary>
        public Link Last { get; }

        /// <summary>
        /// <see cref="Link"/> to the previous page of result
        /// </summary>
        public Link Previous { get;}
    }
}

