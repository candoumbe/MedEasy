namespace Measures.CQRS.Handlers.BloodPressures
{
    using AutoMapper.QueryableExtensions;

    using DataFilters;

    using Measures.CQRS.Queries.BloodPressures;
    using Measures.DTO;
    using Measures.Ids;
    using Measures.Objects;

    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
    using MedEasy.RestObjects;

    using MediatR;

    using Optional;

    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="GetPageOfBloodPressureInfoBySubjectIdQuery"/>s
    /// </summary>
    public class HandleGetPageOfBloodPressureInfoByPatientIdQuery : IRequestHandler<GetPageOfBloodPressureInfoBySubjectIdQuery, Option<Page<BloodPressureInfo>>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetPageOfBloodPressureInfoQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetPageOfBloodPressureInfoByPatientIdQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }


        public async Task<Option<Page<BloodPressureInfo>>> Handle(GetPageOfBloodPressureInfoBySubjectIdQuery query, CancellationToken cancellationToken)
        {
            (SubjectId patientId, PaginationConfiguration pagination) = query.Data;
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            Option<Page<BloodPressureInfo>> optionalResult = Option.None<Page<BloodPressureInfo>>();

            if (await uow.Repository<Subject>().AnyAsync(patient => patient.Id == patientId, cancellationToken).ConfigureAwait(false))
            {
                Expression<Func<BloodPressure, BloodPressureInfo>> selector = _expressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();
                optionalResult = Option.Some(await uow.Repository<BloodPressure>()
                                        .WhereAsync(selector: selector,
                                                    predicate: (BloodPressureInfo p) => p.SubjectId == patientId,
                                                    orderBy: new Sort<BloodPressureInfo>(nameof(BloodPressureInfo.DateOfMeasure), SortDirection.Descending),
                                                    pageSize: pagination.PageSize,
                                                    page: pagination.Page,
                                                    cancellationToken)
                                        .ConfigureAwait(false));
            }

            return optionalResult;
        }
    }
}
