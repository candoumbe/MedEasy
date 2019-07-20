using AutoMapper.QueryableExtensions;
using Measures.CQRS.Queries.BloodPressures;
using Measures.DTO;
using Measures.Objects;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers.BloodPressures
{
    /// <summary>
    /// Handles <see cref="GetBloodPressureInfoByIdQuery"/>s
    /// </summary>
    public class HandleGetOneBloodPressureInfoByIdQuery : IRequestHandler<GetBloodPressureInfoByIdQuery, Option<BloodPressureInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetOneBloodPressureInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/> or <paramref name="expressionBuilder"/> is <c>null</c></exception>
        public HandleGetOneBloodPressureInfoByIdQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }


        public async Task<Option<BloodPressureInfo>> Handle(GetBloodPressureInfoByIdQuery query, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Expression<Func<BloodPressure, BloodPressureInfo>> selector = _expressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();
                return await uow.Repository<BloodPressure>()
                    .SingleOrDefaultAsync(
                        selector,
                        (BloodPressure x) => x.Id == query.Data,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
