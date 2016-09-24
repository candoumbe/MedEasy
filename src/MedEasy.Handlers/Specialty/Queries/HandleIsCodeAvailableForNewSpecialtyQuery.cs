using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using MedEasy.Queries.Specialty;
using System;

namespace MedEasy.Handlers.Specialty.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IIsCodeAvailableForNewSpecialtyQuery"/> interface implementations
    /// </summary>
    public class HandleIsCodeAvailableForNewSpecialtyQuery : IHandleIsCodeAvailableForNewSpecialtyQuery
    {

        private readonly ILogger<HandleIsCodeAvailableForNewSpecialtyQuery> _logger;
        private readonly IUnitOfWorkFactory _factory;

        /// <summary>
        /// Builds a new <see cref="HandleIsCodeAvailableForNewSpecialtyQuery"/> instance.
        /// </summary>
        /// <param name="factory">Factory for creating instances of <see cref="IUnitOfWork"/></param>
        /// <param name="logger">Logger</param>
        /// <exception cref="ArgumentNullException">if <paramref name="factory"/> or <paramref name="logger"/> is <c>null</c></exception>
        public HandleIsCodeAvailableForNewSpecialtyQuery(IUnitOfWorkFactory factory, ILogger<HandleIsCodeAvailableForNewSpecialtyQuery> logger)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            _factory = factory;
            _logger = logger;
        }

        public  async Task<bool> HandleAsync(IIsCodeAvailableForNewSpecialtyQuery query)
        {

            _logger.LogInformation($"Entering {nameof(HandleIsCodeAvailableForNewSpecialtyQuery)}.{nameof(HandleAsync)}({nameof(query)}):'{query}'");

            string code = query.Data?.Trim().ToUpper() ?? string.Empty;
            bool available = false;

            if (!string.IsNullOrWhiteSpace(query.Data))   
            {
                using (var uow = _factory.New())
                {
                    available = ! await uow.Repository<Objects.Specialty>()
                        .AnyAsync(item => item.Code.ToUpper() == code);
                }
                
            }

            _logger.LogInformation($"Exiting {nameof(HandleIsCodeAvailableForNewSpecialtyQuery)}.{nameof(HandleAsync)}({nameof(query)}");
            return available;



        }
    }
}