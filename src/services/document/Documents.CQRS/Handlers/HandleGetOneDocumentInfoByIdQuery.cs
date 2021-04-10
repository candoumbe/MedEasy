using Documents.CQRS.Queries;
using Documents.DTO.v1;
using Documents.Objects;

using MedEasy.DAL.Interfaces;

using MediatR;

using Optional;

using System.Threading;
using System.Threading.Tasks;

namespace Documents.CQRS.Handlers
{
    public class HandleGetOneDocumentInfoByIdQuery: IRequestHandler<GetOneDocumentInfoByIdQuery, Option<DocumentInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Builds a <see cref="HandleCreateDocumentInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// 
        public HandleGetOneDocumentInfoByIdQuery(IUnitOfWorkFactory uowFactory)
        {
            _uowFactory = uowFactory;
        }

        public async Task<Option<DocumentInfo>> Handle(GetOneDocumentInfoByIdQuery request, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
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
}
