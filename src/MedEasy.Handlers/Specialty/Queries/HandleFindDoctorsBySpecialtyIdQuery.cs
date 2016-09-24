using System.Threading.Tasks;
using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.Queries.Specialty;
using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using MedEasy.RestObjects;

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
            if (uowFactory == null)
            {
                throw new ArgumentNullException(nameof(uowFactory));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            UowFactory = uowFactory;
            Logger = logger;

        }

        public Task<IPagedResult<DoctorInfo>> HandleAsync(IFindDoctorsBySpecialtyIdQuery query)
        {
            Logger.LogInformation($"Starting {nameof(IFindDoctorsBySpecialtyIdQuery)}  query handling");
            if (query == null)
            {
                Logger.LogTrace("query to handle is null");
                throw new ArgumentNullException(nameof(query));
            }
            GenericGetQuery getQuery = query.Data?.GetQuery ?? new GenericGetQuery();
            //getQuery.PageSize = Math.Min()

            return Task.FromResult((IPagedResult<DoctorInfo>)new PagedResult<DoctorInfo>(Enumerable.Empty<DoctorInfo>(), 0, query.Data.GetQuery.PageSize));
        }
    }
}
