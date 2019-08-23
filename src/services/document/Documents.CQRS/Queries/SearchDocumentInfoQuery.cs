using Documents.DTO.v1;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using System;

namespace Documents.CQRS.Queries
{
    /// <summary>
    /// Query to retrieve a document based on <see cref="DocumentInfo"/>"s criteria.
    /// </summary>
    public class SearchDocumentInfoQuery : QueryBase<Guid, SearchDocumentInfo, Page<DocumentInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="SearchDocumentInfoQuery"/> instance
        /// </summary>
        /// <param name="data">Criterion to search <see cref="DocumentInfo"/>s</param>
        /// <exception cref="ArgumentNullException">if <paramref name="data"/> is <c>null</c>.</exception>
        public SearchDocumentInfoQuery(SearchDocumentInfo data) : base(Guid.NewGuid(), data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }
}
