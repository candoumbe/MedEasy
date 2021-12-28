namespace Measures.CQRS.Handlers.Subjects
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
    public class HandleGetPageOfBloodPressureInfoBySubjectIdQuery : IRequestHandler<GetPageOfBloodPressureInfoBySubjectIdQuery, Option<Page<BloodPressureInfo>>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="GetPageOfBloodPressureInfoBySubjectIdQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetPageOfBloodPressureInfoBySubjectIdQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

        public async Task<Option<Page<BloodPressureInfo>>> Handle(GetPageOfBloodPressureInfoBySubjectIdQuery query, CancellationToken cancellationToken)
        {
            (SubjectId subjectId, PaginationConfiguration pagination) = query.Data;
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            Option<Page<BloodPressureInfo>> result;
            if (await uow.Repository<Subject>().AnyAsync(x => x.Id == subjectId, cancellationToken).ConfigureAwait(false))
            {
                Expression<Func<BloodPressure, BloodPressureInfo>> selector = _expressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();
                result = Option.Some(await uow.Repository<BloodPressure>()
                            .WhereAsync(
                                selector,
                                (BloodPressure x) => x.Id == subjectId,
                                new Sort<BloodPressureInfo>(nameof(BloodPressureInfo.UpdatedDate), SortDirection.Descending),
                                pagination.PageSize,
                                pagination.Page,
                                cancellationToken)
                            .ConfigureAwait(false));
            }
            else
            {
                result = Option.None<Page<BloodPressureInfo>>();
            }

            return result;
        }
    }
}
