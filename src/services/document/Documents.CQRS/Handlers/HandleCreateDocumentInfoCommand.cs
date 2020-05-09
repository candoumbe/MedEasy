using AutoMapper;
using Documents.CQRS.Commands;
using Documents.DTO.v1;
using Documents.Objects;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Documents.CQRS.Handlers
{
    public class HandleCreateDocumentInfoCommand : IRequestHandler<CreateDocumentInfoCommand, Option<DocumentInfo, CreateCommandResult>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Builds a <see cref="HandleCreateDocumentInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// 
        public HandleCreateDocumentInfoCommand(IUnitOfWorkFactory uowFactory)
        {
            _uowFactory = uowFactory;
        }

        public async Task<Option<DocumentInfo, CreateCommandResult>> Handle(CreateDocumentInfoCommand request, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

            Document document = new Document(id: request.Data.Id == Guid.Empty ? Guid.NewGuid() : request.Data.Id,
                name: request.Data.Name);

            if (!string.IsNullOrWhiteSpace(request.Data.MimeType))
            {
                document.ChangeMimeTypeTo(request.Data.MimeType);
            }

            uow.Repository<Document>().Create(document);
            uow.Repository<DocumentPart>().Create(new DocumentPart(document.Id, 0, request.Data.Content));

            document.UpdateSize(request.Data.Content.Length);
            document.UpdateHash(BitConverter.ToString(SHA256.Create().ComputeHash(request.Data.Content)));
            document.Lock();

            await uow.SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            return new DocumentInfo
            {
                MimeType = document.MimeType,
                CreatedDate = document.CreatedDate,
                Hash = document.Hash,
                Id = document.Id,
                Name = document.Name,
                UpdatedDate = document.UpdatedDate
            }.Some<DocumentInfo, CreateCommandResult>();
        }
    }
}
