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
using System.Threading;

namespace MedEasy.Handlers.Specialty.Queries
{
    /// <summary>
    /// Handler of <see cref="IFindDoctorsBySpecialtyIdQuery"/> instances
    /// </summary>
    public class HandleFindDoctorsBySpecialtyIdQuery : IHandleFindDoctorsBySpecialtyIdQuery
    {
        private IUnitOfWorkFactory UowFactory { get; }
        private ILogger<HandleFindDoctorsBySpecialtyIdQuery> Logger { get; }
        
        /// <summary>
        /// Builds a new <see cref="HandleFindDoctorsBySpecialtyIdQuery"/> instance
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="logger"></param>
        /// <remarks>
        /// This handler is thread safe
        /// </remarks>
        /// <exception cref="ArgumentNullException">if <paramref name="uowFactory"/> or <paramref name="logger"/> is <c>null</c></exception>
        public HandleFindDoctorsBySpecialtyIdQuery(IUnitOfWorkFactory uowFactory, ILogger<HandleFindDoctorsBySpecialtyIdQuery> logger)
        {
            UowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<IPagedResult<DoctorInfo>> HandleAsync(IFindDoctorsBySpecialtyIdQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            Logger.LogInformation($"Starting {nameof(IFindDoctorsBySpecialtyIdQuery)}  query handling");
            if (query == null)
            {
                Logger.LogTrace("query to handle is null");
                throw new ArgumentNullException(nameof(query));
            }
            PaginationConfiguration getQuery = query.Data?.GetQuery ?? new PaginationConfiguration();
            
            using (IUnitOfWork uow = UowFactory.New())
            {
                //IPagedResult<DoctorInfo> pageOfResult = await uow.Repository<Objects.Doctor>()
                //    .WhereAsync(
                //        x => new DoctorInfo
                //        {
                //            Firstname = x.Firstname,
                //            Lastname = x.Lastname,
                //            Id = x.UUID,
                //            SpecialtyId = x.Specialty != null ? x.Specialty.UUID : (Guid?)null,
                //            UpdatedDate = x.UpdatedDate
                //        },  
                //        (DoctorInfo x) => x.SpecialtyId == query.Data.SpecialtyId,
                //        Enumerable.Empty<OrderClause<DoctorInfo>>(), 
                //        getQuery.PageSize, getQuery.Page);

                //return pageOfResult;

                IPagedResult<Objects.Doctor> pageOfResult = await uow.Repository<Objects.Doctor>()
                    .WhereAsync(x => x.Specialty.UUID == query.Data.SpecialtyId,
                        Enumerable.Empty<OrderClause<Objects.Doctor>>(),
                        getQuery.PageSize, getQuery.Page);


                // TODO Move the selector into the query 
                return new PagedResult<DoctorInfo>(pageOfResult.Entries.Select(x => new DoctorInfo
                {
                    Firstname = x.Firstname,
                    Lastname = x.Lastname,
                    Id = x.UUID,
                    SpecialtyId = x.Specialty != null ? x.Specialty.UUID : (Guid?)null,
                    UpdatedDate = x.UpdatedDate
                }), pageOfResult.Total, pageOfResult.PageSize);

            }
        }
    }
}
