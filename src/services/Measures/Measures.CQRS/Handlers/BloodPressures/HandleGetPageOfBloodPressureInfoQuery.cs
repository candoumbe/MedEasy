using AutoMapper.QueryableExtensions;
using Measures.CQRS.Commands.BloodPressures;
using Measures.CQRS.Queries.BloodPressures;
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

namespace Measures.CQRS.Handlers.BloodPressures
{
    /// <summary>
    /// Handles <see cref="GetPageOfBloodPressureInfoQuery"/>s
    /// </summary>
    public class HandleGetPageOfBloodPressureInfoQuery : IRequestHandler<GetPageOfBloodPressureInfoQuery, Page<BloodPressureInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetPageOfBloodPressureInfoQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetPageOfBloodPressureInfoQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }


        public async Task<Page<BloodPressureInfo>> Handle(GetPageOfBloodPressureInfoQuery query, CancellationToken cancellationToken)
        {
            PaginationConfiguration pagination = query.Data;
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Expression<Func<BloodPressure, BloodPressureInfo>> selector = _expressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();
                Page<BloodPressureInfo> result = await uow.Repository<BloodPressure>()
                    .ReadPageAsync(
                        selector,
                        pagination.PageSize,
                        pagination.Page,
                        new[] { OrderClause<BloodPressureInfo>.Create(x => x.DateOfMeasure) },
                        cancellationToken)
                    .ConfigureAwait(false);


                return result;
            }
        }
    }
}
