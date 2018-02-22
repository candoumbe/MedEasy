namespace MedEasy.RestObjects
{

    /// <summary>
    /// Link relation type representation (see http://www.iana.org/assignments/link-relations/link-relations.xhtml)
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    public class LinkRelation
    {
        /// <summary>
        /// 
        /// </summary>
        public const string Self = "self";

        /// <summary>
        /// An IRI that refers to the furthest preceding resource in a series of resources.
        /// </summary>
        public const string First = "first";

        /// <summary>
        /// Indicates that the link's context is a part of a series, and that the previous in the series is the link target
        /// </summary>
        public const string Previous = "previous";

        /// <summary>
        /// Indicates that the link's context is a part of a series, and that the next in the series is the link target
        /// </summary>
        public const string Next = "next";

        /// <summary>
        /// An IRI that refers to the furthest following resource in a series of resources.
        /// </summary>
        public const string Last = "last";

        /// <summary>
        /// Refers to a resource that can be used to search through the link's context and related resources.
        /// </summary>
        public const string Search = "search";

        /// <summary>
        /// The target IRI points to a resource which represents the collection resource for the context IRI.
        /// </summary>
        public const string Collection = "collection";

        /// <summary>
        /// The target IRI points to a resource where a submission form can be obtained.
        /// </summary>
        public const string CreateForm = "create-form";
        
        /// <summary>
        /// The target IRI points to a resource where a submission form for editing associated resource can be obtained.
        /// </summary>
        public const string EditForm = "edit-form";

    }
}
