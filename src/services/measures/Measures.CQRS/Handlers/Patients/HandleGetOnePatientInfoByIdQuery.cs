namespace Measures.CQRS.Handlers.Patients
{
    using AutoMapper.QueryableExtensions;

    using Measures.CQRS.Queries.Patients;
    using Measures.DTO;
    using Measures.Objects;

    using MedEasy.DAL.Interfaces;

    using MediatR;

    using Optional;

    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="GetPatientInfoByIdQuery"/>s
    /// </summary>
    public class HandleGetOnePatientInfoByIdQuery : IRequestHandler<GetPatientInfoByIdQuery, Option<PatientInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleCreatePatientInfoCommand"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/> or <paramref name="expressionBuilder"/> is <c>null</c></exception>
        public HandleGetOnePatientInfoByIdQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }


        public async Task<Option<PatientInfo>> Handle(GetPatientInfoByIdQuery query, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            Expression<Func<Patient, PatientInfo>> selector = _expressionBuilder.GetMapExpression<Patient, PatientInfo>();
            Option<PatientInfo> result = await uow.Repository<Patient>()
                .SingleOrDefaultAsync(
                    selector,
                    (Patient x) => x.Id == query.Data,
                    cancellationToken)
                .ConfigureAwait(false);


            return result;
        }
    }
}
