using Documents.CQRS.Commands;
using Documents.Objects;

using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;

using MediatR;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Documents.CQRS.Handlers
{
    public class HandleDeleteDocumentInfoByIdCommand : IRequestHandler<DeleteDocumentInfoByIdCommand, DeleteCommandResult>
    {
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Builds a <see cref="HandleCreateDocumentInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// 
        public HandleDeleteDocumentInfoByIdCommand(IUnitOfWorkFactory uowFactory)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
        }

        public async Task<DeleteCommandResult> Handle(DeleteDocumentInfoByIdCommand request, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Document>().Delete(x => x.Id == request.Data);
            await uow.SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            return DeleteCommandResult.Done;
        }
    }
}
