using AutoMapper.QueryableExtensions;
using Measures.CQRS.Commands;
using Measures.DTO;
using Measures.Objects;
using MedEasy.CQRS.Core.Commands;
using MedEasy.DAL.Interfaces;
using MediatR;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers
{
    /// <summary>
    /// Handles <see cref="DeleteBloodPressureInfoByIdCommand"/>s
    /// </summary>
    public class HandleDeleteBloodPressureInfoByIdCommand : IRequestHandler<DeleteBloodPressureInfoByIdCommand, DeleteCommandResult>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleCreateBloodPressureInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleDeleteBloodPressureInfoByIdCommand(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }


        public async Task<DeleteCommandResult> Handle(DeleteBloodPressureInfoByIdCommand cmd, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.New())
            {
                DeleteCommandResult result = DeleteCommandResult.Done;
                if (await uow.Repository<BloodPressure>().AnyAsync(x => x.UUID == cmd.Data).ConfigureAwait(false))
                {
                    uow.Repository<BloodPressure>().Delete(x => x.UUID == cmd.Data);
                    await uow.SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    result = DeleteCommandResult.Failed_NotFound;
                }

                return result;
            }
        }
    }
}
