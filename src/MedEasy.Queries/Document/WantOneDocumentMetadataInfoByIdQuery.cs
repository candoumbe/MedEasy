using MedEasy.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Queries.Document
{
    /// <summary>
    /// A query to get one <see cref="DocumentMetadataInfo"/> by its id.
    /// </summary>
    public class WantOneDocumentMetadataInfoByIdQuery : GenericGetOneResourceByIdQuery<int, DocumentMetadataInfo>, IWantOneDocumentMetadataInfoByIdQuery
    {
        /// <summary>
        /// Builds a new <see cref="WantOneDocumentMetadataInfoByIdQuery"/>
        /// </summary>
        /// <param name="id">id of the document to query.</param>
        public WantOneDocumentMetadataInfoByIdQuery(int id) : base(id)
        {
        }
    }
}
