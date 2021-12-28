namespace Measures.CQRS.Handlers.Subjects
{
    using AutoMapper.QueryableExtensions;

    using DataFilters;

    using Measures.CQRS.Queries.Subjects;
    using Measures.DTO;
    using Measures.Objects;

    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
    using MedEasy.RestObjects;

    using MediatR;

    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles <see cref="GetPageOfSubjectInfoQuery"/>s
    /// </summary>
    public class HandleGetPageOfSubjectInfoQuery : IRequestHandler<GetPageOfSubjectInfoQuery, Page<SubjectInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetPageOfSubjectInfoQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetPageOfSubjectInfoQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }


        public async Task<Page<SubjectInfo>> Handle(GetPageOfSubjectInfoQuery query, CancellationToken cancellationToken)
        {
            PaginationConfiguration pagination = query.Data;
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            Expression<Func<Subject, SubjectInfo>> selector = _expressionBuilder.GetMapExpression<Subject, SubjectInfo>();
            Page<SubjectInfo> result = await uow.Repository<Subject>()
                .ReadPageAsync(
                    selector,
                    pagination.PageSize,
                    pagination.Page,
                    new Sort<SubjectInfo>(nameof(SubjectInfo.Name), SortDirection.Descending),
                    cancellationToken)
                .ConfigureAwait(false);


            return result;
        }
    }
}
