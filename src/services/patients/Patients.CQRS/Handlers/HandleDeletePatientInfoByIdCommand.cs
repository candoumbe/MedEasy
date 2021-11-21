namespace Patients.CQRS.Handlers.Patients
{
    using AutoMapper.QueryableExtensions;

    using DataFilters;

    using global::Patients.CQRS.Commands;
    using global::Patients.CQRS.Queries;
    using global::Patients.DTO;
    using global::Patients.Objects;

    using MassTransit;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;

    using MediatR;

    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="GetPageOfPatientsQuery"/>s
    /// </summary>
    public class HandleDeletePatientInfoByIdCommand : IRequestHandler<DeletePatientInfoByIdCommand, DeleteCommandResult>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IMediator _mediator;
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// Builds a new <see cref="HandleCreatePatientInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <param name="mediator">Mediator</param>
        /// <paramref name="mediator"/> is <c>null</c>.
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/>, <paramref name="expressionBuilder"/> or 
        /// </exception>
        public HandleDeletePatientInfoByIdCommand(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IMediator mediator, IPublishEndpoint publishEndpoint)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }


        ///<inheritdoc/>
        public async Task<DeleteCommandResult> Handle(DeletePatientInfoByIdCommand request, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            
            uow.Repository<Patient>().Delete(x => x.Id == request.Data);

            await uow.SaveChangesAsync(cancellationToken)
                     .ConfigureAwait(false);

            return DeleteCommandResult.Done;
        }
    }
}
