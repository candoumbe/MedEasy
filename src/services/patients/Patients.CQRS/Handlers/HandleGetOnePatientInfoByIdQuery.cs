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

    using Optional;

    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="GetOnePatientInfoByIdQuery"/> queries.
    /// </summary>
    public class HandleGetOnePatientInfoByIdQuery : IRequestHandler<GetOnePatientInfoByIdQuery, Option<PatientInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleCreatePatientInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/>, <paramref name="expressionBuilder"> is <c>null</c>.</exception>
        public HandleGetOnePatientInfoByIdQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

        ///<inheritdoc/>
        public async Task<Option<PatientInfo>> Handle(GetOnePatientInfoByIdQuery request, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            Expression<Func<Patient, PatientInfo>> mapEntityToDtoExpression = _expressionBuilder.GetMapExpression<Patient, PatientInfo>();
            
            return await uow.Repository<Patient>().SingleOrDefaultAsync(mapEntityToDtoExpression,
                                                                        (Patient x) => x.Id == request.Data,
                                                                        cancellationToken);
        }
    }
}
