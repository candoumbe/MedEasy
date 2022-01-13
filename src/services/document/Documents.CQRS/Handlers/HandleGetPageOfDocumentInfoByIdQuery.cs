namespace Documents.CQRS.Handlers
{
    using Documents.CQRS.Queries;
    using Documents.DTO.v1;
    using Documents.Objects;

    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;

    using MediatR;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="GetPageOfDocumentInfoQuery"/> queries.
    /// </summary>
    public class HandleGetPageOfDocumentInfoQuery : IRequestHandler<GetPageOfDocumentInfoQuery, Page<DocumentInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Builds a <see cref="HandleCreateDocumentInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        public HandleGetPageOfDocumentInfoQuery(IUnitOfWorkFactory uowFactory)
        {
            _uowFactory = uowFactory;
        }

        ///<inheritdoc/>
        public async Task<Page<DocumentInfo>> Handle(GetPageOfDocumentInfoQuery request, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            return await uow.Repository<Document>()
                            .ReadPageAsync(
                                selector: doc => new DocumentInfo
                                {
                                    Name = doc.Name,
                                    Id = doc.Id,
                                    MimeType = doc.MimeType,
                                    Hash = doc.Hash,
                                    CreatedDate = doc.CreatedDate,
                                    UpdatedDate = doc.UpdatedDate
                                },
                                pageSize: request.Data.PageSize,
                                page: request.Data.Page,
                                orderBy: nameof(DocumentInfo.Name).ToSort<DocumentInfo>(),
                                ct: cancellationToken)
                            .ConfigureAwait(false);
        }
    }
}
