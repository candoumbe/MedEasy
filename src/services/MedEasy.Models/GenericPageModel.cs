using System.Collections.Generic;
using System.Linq;

namespace MedEasy.Models
{
    public class GenericPageModel<T> : PageModelBase
    {
        /// <summary>
        /// Links that helps navigated through pages of the result
        /// </summary>
        public PageLinksModel Links { get; set; }

        /// <summary>
        /// The items of the current page of result
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Number of items in the the result
        /// </summary>
        
        private long? _count;

        public override long Count
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
    }
}
