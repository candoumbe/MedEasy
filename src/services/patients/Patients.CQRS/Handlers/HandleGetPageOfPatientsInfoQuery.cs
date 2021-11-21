namespace Patients.CQRS.Handlers.Patients
{
    using AutoMapper.QueryableExtensions;

    using DataFilters;

    using global::Patients.CQRS.Queries;
    using global::Patients.DTO;
    using global::Patients.Objects;

    using MassTransit;

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
    public class HandleGetPageOfPatientsInfoQuery : IRequestHandler<GetPageOfPatientsQuery, Page<PatientInfo>>
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
        public HandleGetPageOfPatientsInfoQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder, IMediator mediator, IPublishEndpoint publishEndpoint)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }


        ///<inheritdoc/>
        public async Task<Page<PatientInfo>> Handle(GetPageOfPatientsQuery request, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            Expression<Func<Patient, PatientInfo>> mapEntityToDtoExpression = _expressionBuilder.GetMapExpression<Patient, PatientInfo>();
            return await uow.Repository<Patient>().ReadPageAsync(mapEntityToDtoExpression,
                                                                 request.Data.PageSize,
                                                                 request.Data.Page,
                                                                 new Sort<Patient>(nameof(Patient.CreatedDate), SortDirection.Descending),
                                                                 cancellationToken);
        }
    }
}
