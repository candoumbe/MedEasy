using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using MedEasy.Queries.Specialty;
using System;
using MedEasy.Handlers.Core.Specialty.Queries;
using System.Threading;

namespace MedEasy.Handlers.Specialty.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IIsNameAvailableForNewSpecialtyQuery"/> interface implementations
    /// </summary>
    public class HandleIsNameAvailableForNewSpecialtyQuery : IHandleIsNameAvailableForNewSpecialtyQuery
    {

        private readonly ILogger<HandleIsNameAvailableForNewSpecialtyQuery> _logger;
        private readonly IUnitOfWorkFactory _factory;

        /// <summary>
        /// Builds a new <see cref="HandleIsNameAvailableForNewSpecialtyQuery"/> instance.
        /// </summary>
        /// <param name="factory">Factory for creating instances of <see cref="IUnitOfWork"/></param>
        /// <param name="logger">Logger</param>
        /// <exception cref="ArgumentNullException">if <paramref name="factory"/> or <paramref name="logger"/> is <c>null</c></exception>
        public HandleIsNameAvailableForNewSpecialtyQuery(IUnitOfWorkFactory factory, ILogger<HandleIsNameAvailableForNewSpecialtyQuery> logger)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public  async Task<bool> HandleAsync(IIsNameAvailableForNewSpecialtyQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {

            _logger.LogInformation($"Entering {nameof(HandleIsNameAvailableForNewSpecialtyQuery)}.{nameof(HandleAsync)}({nameof(query)}):'{query}'");

            string name = query.Data?.Trim().ToUpper() ?? string.Empty;
            bool available = false;

            if (!string.IsNullOrWhiteSpace(query.Data))   
            {
                using (IUnitOfWork uow = _factory.New())
                {
                    available = ! await uow.Repository<Objects.Specialty>()
                        .AnyAsync(item => item.Name.ToUpper() == name, cancellationToken)
                        .ConfigureAwait(false);
                }
                
            }

            _logger.LogInformation($"Exiting {nameof(HandleIsNameAvailableForNewSpecialtyQuery)}.{nameof(HandleAsync)}({nameof(query)}");
            return available;



        }
    }
}