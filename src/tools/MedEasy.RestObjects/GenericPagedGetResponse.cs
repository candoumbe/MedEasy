namespace MedEasy.RestObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Wraps a paged response
    /// </summary>
    /// <typeparam name="T">Type of items that will be wrapped in a paged result</typeparam>

    public class GenericPagedGetResponse<T> : IGenericPagedGetResponse
    {
        /// <summary>
        /// Links that helps navigated through pages of the result
        /// </summary>
        public PageLinks Links { get; }

        /// <summary>
        /// Builds a new <see cref="GenericPagedGetResponse{T}"/> instance. 
        /// </summary>
        /// <param name="items">items of the current page</param>
        /// <param name="first"><see cref="Link"/> to the first page of response</param>
        /// <param name="previous"><see cref="Link"/> to the previous page of response</param>
        /// <param name="next"><see cref="Link"/> to the next page of response</param>
        /// <param name="last"><see cref="Link"/> to the last page of response</param>
        /// <param name="total">Total count of items</param>
        public GenericPagedGetResponse(in IEnumerable<T> items,
                                       in string first = null,
                                       in string previous = null,
                                       in string next = null,
                                       string last = null,
                                       in long total = 0)
        {
            Items = items;
            Links = new PageLinks(first, previous, next, last);
            Total = total;
        }
        /// <summary>
        /// The items of the current page of result
        /// </summary>
        public IEnumerable<T> Items { get; }

        /// <summary>
        /// Number of items in the result
        /// </summary>
        public long Total { get; }

        private long? _count;

        public long Count
        {
            get
            {
                if (!_count.HasValue)
                {
                    _count = Items?.Count() ?? 0;
                }

                return _count ?? 0;
            }
        }

        public override string ToString() => this.Jsonify();
    }
}