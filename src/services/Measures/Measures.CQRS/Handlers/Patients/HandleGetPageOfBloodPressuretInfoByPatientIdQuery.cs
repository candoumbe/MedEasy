using AutoMapper.QueryableExtensions;
using DataFilters;
using Measures.CQRS.Queries.BloodPressures;
using Measures.CQRS.Queries.Patients;
using Measures.DTO;
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

namespace Measures.CQRS.Handlers.Patients
{
    /// <summary>
    /// Handles <see cref="GetPageOfBloodOPressureInfoByPatientIdQuery"/>s
    /// </summary>
    public class HandleGetPageOfBloodPressureInfoByPatientIdQuery : IRequestHandler<GetPageOfBloodPressureInfoByPatientIdQuery, Option<Page<BloodPressureInfo>>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="GetPageOfBloodOPressureInfoByPatientIdQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetPageOfBloodPressureInfoByPatientIdQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

        public async Task<Option<Page<BloodPressureInfo>>> Handle(GetPageOfBloodPressureInfoByPatientIdQuery query, CancellationToken cancellationToken)
        {
            (Guid patientId, PaginationConfiguration pagination) = query.Data;
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Option<Page<BloodPressureInfo>> result;
                if (await uow.Repository<Patient>().AnyAsync(x => x.Id == patientId).ConfigureAwait(false))
                {
                    Expression<Func<BloodPressure, BloodPressureInfo>> selector = _expressionBuilder.GetMapExpression<BloodPressure, BloodPressureInfo>();
                    result = Option.Some( await uow.Repository<BloodPressure>()
                                .WhereAsync(
                                    selector,
                                    (BloodPressure x) => x.Id == patientId,
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
}
