using AutoMapper.QueryableExtensions;
using DataFilters;
using Measures.CQRS.Queries.Patients;
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

namespace Measures.CQRS.Handlers.Patients
{
    /// <summary>
    /// Handles <see cref="GetPageOfPatientInfoQuery"/>s
    /// </summary>
    public class HandleGetPageOfPatientInfoQuery : IRequestHandler<GetPageOfPatientInfoQuery, Page<PatientInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetPageOfPatientInfoQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetPageOfPatientInfoQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }


        public async Task<Page<PatientInfo>> Handle(GetPageOfPatientInfoQuery query, CancellationToken cancellationToken)
        {
            PaginationConfiguration pagination = query.Data;
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Expression<Func<Patient, PatientInfo>> selector = _expressionBuilder.GetMapExpression<Patient, PatientInfo>();
                Page<PatientInfo> result = await uow.Repository<Patient>()
                    .ReadPageAsync(
                        selector,
                        pagination.PageSize,
                        pagination.Page,
                        new Sort<PatientInfo>(nameof(PatientInfo.UpdatedDate), SortDirection.Descending).ToOrderClause(),
                        cancellationToken)
                    .ConfigureAwait(false);


                return result;
            }
        }
    }
}
