﻿namespace MedEasy.RestObjects
{
    using System;

    /// <summary>
    /// An instance of this class holds parameters of a "GET" query.
    /// </summary>
    /// <remarks>
    /// Both <see cref="PageSize"/> and <see cref="Page"/> returns <c>null</c> or positive integer
    /// </remarks>
    public class PaginationConfiguration : IEquatable<PaginationConfiguration>
    {
        /// <summary>
        /// Size of a page if none provided
        /// </summary>
        public const int DefaultPageSize = 30;
        /// <summary>
        /// Max size of the page
        /// </summary>
        public const int MaxPageSize = 200;

        /// <summary>
        /// Builds a new <see cref="PaginationConfiguration"/> instance.
        /// </summary>
        /// <remarks>
        /// <see cref="Page"/> is set to <c>1</c> and <see cref="PageSize"/> is set to <see cref="DefaultPageSize"/> value
        /// </remarks>
        public PaginationConfiguration()
        {
            PageSize = DefaultPageSize;
            Page = 1;
        }

        /// <summary>
        /// Gets/Sets the maximum number of resource to get. 
        /// </summary>
        /// <remarks>
        /// Number of items a page can contain at most.
        /// </remarks>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets/Sets the page of data to retrieve.
        /// </summary>
        /// <remarks>
        /// The page is set to <c>1</c> whenever the value is less than 1
        /// </remarks>
        public int Page { get; set; }

        ///<inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as PaginationConfiguration);

        ///<inheritdoc/>
        public override int GetHashCode() => (Page, PageSize).GetHashCode();

        ///<inheritdoc/>
        public bool Equals(PaginationConfiguration other) => (Page, PageSize) == (other?.Page, other?.PageSize);

        ///<inheritdoc/>
        public override string ToString() => this.Jsonify();
    }
}