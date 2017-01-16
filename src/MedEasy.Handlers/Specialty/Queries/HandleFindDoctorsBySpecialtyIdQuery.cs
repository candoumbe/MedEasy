using System.Threading.Tasks;
using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Queries.Specialty;
using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using MedEasy.RestObjects;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Core.Specialty.Queries;

namespace MedEasy.Handlers.Specialty.Queries
{
    /// <summary>
    /// Handler of <see cref="IFindDoctorsBySpecialtyIdQuery"/> instances
    /// </summary>
    public class HandleFindDoctorsBySpecialtyIdQuery : IHandleFindDoctorsBySpecialtyIdQuery
    {
        private IUnitOfWorkFactory UowFactory { get; }
        private ILogger<HandleFindDoctorsBySpecialtyIdQuery> Logger { get; }

        private IExpressionBuilder ExpressionBuilder { get; }

        /// <summary>
        /// Builds a new <see cref="HandleFindDoctorsBySpecialtyIdQuery"/> instance
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="logger"></param>
        /// <remarks>
        /// This handler is thread safe
        /// </remarks>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/> or <paramref name="logger"/> is <c>null</c></exception>
        public HandleFindDoctorsBySpecialtyIdQuery(IUnitOfWorkFactory uowFactory, ILogger<HandleFindDoctorsBySpecialtyIdQuery> logger, IExpressionBuilder expressionBuilder)
        {
            if (uowFactory == null)
            {
                throw new ArgumentNullException(nameof(uowFactory));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (expressionBuilder == null)
            {
                throw new ArgumentNullException(nameof(expressionBuilder));
            }

            UowFactory = uowFactory;
            Logger = logger;
            ExpressionBuilder = expressionBuilder;

        }

        public async Task<IPagedResult<DoctorInfo>> HandleAsync(IFindDoctorsBySpecialtyIdQuery query)
        {
            Logger.LogInformation($"Starting {nameof(IFindDoctorsBySpecialtyIdQuery)}  query handling");
            if (query == null)
            {
                Logger.LogTrace("query to handle is null");
                throw new ArgumentNullException(nameof(query));
            }
            PaginationConfiguration getQuery = query.Data?.GetQuery ?? new PaginationConfiguration();
            
            using (var uow = UowFactory.New())
            { 
                IPagedResult<DoctorInfo> pageOfResult = await uow.Repository<Objects.Doctor>()
                    .WhereAsync(
                        ExpressionBuilder.CreateMapExpression<Objects.Doctor, DoctorInfo>(),      //selector  
                        (Objects.Doctor x) => x.SpecialtyId == query.Data.SpecialtyId,
                        Enumerable.Empty<OrderClause<DoctorInfo>>(), 
                        getQuery.PageSize, getQuery.Page);


                return pageOfResult;
            }
        }
    }
}
