using MedEasy.DAL.Interfaces;
using MedEasy.Handlers.Core.Autocomplete.Queries;
using MedEasy.Queries.Autocomplete;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MedEasy.Handlers.Autocomplete
{
    public class HandleAutocompleteCityNameQuery : IHandleAutocompleteCityNameQuery
    {
        private IUnitOfWorkFactory Factory { get; }
        private ILogger Logger { get; }

        public HandleAutocompleteCityNameQuery(IUnitOfWorkFactory uowFactory, ILoggerFactory loggerFactory)
        {
            Factory = uowFactory;
            Logger = loggerFactory.CreateLogger<HandleAutocompleteCityNameQuery>();
        }


        public async ValueTask<IEnumerable<string>> HandleAsync(IWantAutocompleteCityNameQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (IUnitOfWork uow = Factory.New())
            {
                return (await uow.Repository<Objects.Patient>()
                    .WhereAsync(p => p.BirthPlace, p => p.BirthPlace != null && p.BirthPlace.Contains(query.Data), cancellationToken))
                    .Distinct()
                    .OrderBy(city => city)
                    .ToArray();
            }
        }
    }
}
