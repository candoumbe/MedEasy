namespace Documents.CQRS.Handlers
{
    using Ardalis.GuardClauses;

    using Documents.CQRS.Queries;
    using Documents.DTO.v1;
    using Documents.Objects;

    using MedEasy.DAL.Interfaces;

    using MediatR;

    using Optional;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="GetOneDocumentInfoByIdQuery"/> queries.
    /// </summary>
    public class HandleGetOneDocumentInfoByIdQuery : IRequestHandler<GetOneDocumentInfoByIdQuery, Option<DocumentInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Builds a <see cref="HandleCreateDocumentInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HandleGetOneDocumentInfoByIdQuery(IUnitOfWorkFactory uowFactory)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
        }

        ///<inheritdoc/>
        public async Task<Option<DocumentInfo>> Handle(GetOneDocumentInfoByIdQuery request, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

            return await uow.Repository<Document>()
                .SingleOrDefaultAsync(
                    selector: doc => new DocumentInfo
                    {
                        Name = doc.Name,
                        Id = doc.Id,
                        MimeType = doc.MimeType,
                        Hash = doc.Hash,
                        CreatedDate = doc.CreatedDate,
                        UpdatedDate = doc.UpdatedDate
                    },
                    predicate: (DocumentInfo doc) => doc.Id == request.Data,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
