using AutoMapper;
using Documents.CQRS.Queries;
using Documents.DTO.v1;
using Documents.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Documents.CQRS.Handlers
{
    public class HandleGetOneDocumentFileInfoByIdQuery : IRequestHandler<GetOneDocumentFileInfoByIdQuery, Option<DocumentFileInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Builds a <see cref="HandleCreateDocumentInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// 
        public HandleGetOneDocumentFileInfoByIdQuery(IUnitOfWorkFactory uowFactory)
        {
            _uowFactory = uowFactory;
        }

        public Task<Option<DocumentFileInfo>> Handle(GetOneDocumentFileInfoByIdQuery request, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

            return uow.Repository<Document>().SingleOrDefaultAsync(
                selector: doc => new DocumentFileInfo
                {
                    Name = doc.Name,
                    Id = doc.Id,
                    MimeType = doc.MimeType,
                    Hash = doc.Hash,
                    CreatedDate = doc.CreatedDate,
                    UpdatedDate = doc.UpdatedDate,
                    Content = doc.File.Content
                },
                predicate: (DocumentFileInfo doc) => doc.Id == request.Data,
                cancellationToken).AsTask();
        }
    }
}
