using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Queries;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using static MedEasy.DAL.Repositories.SortDirection;
using System.Linq;
using MedEasy.Handlers.Core.Prescription.Queries;

namespace MedEasy.Handlers.Prescription.Queries
{
    public class HandleGetMostRecentPrescriptionsQuery : IHandleGetMostRecentPrescriptionsInfo
    {
        private IExpressionBuilder _expressionBuilder;
        private IUnitOfWorkFactory _uowFactory;
        private ILogger<HandleGetMostRecentPrescriptionsQuery> _logger;


        /// <summary>
        /// Builds a new <see cref="HandleGetMostRecentPrescriptionsQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="logger"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetMostRecentPrescriptionsQuery(IUnitOfWorkFactory uowFactory, ILogger<HandleGetMostRecentPrescriptionsQuery> logger, IExpressionBuilder expressionBuilder)
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
            _uowFactory = uowFactory;
            _logger = logger;
            _expressionBuilder = expressionBuilder;
        }

        public async Task<IEnumerable<PrescriptionHeaderInfo>> HandleAsync(IQuery<Guid, GetMostRecentPrescriptionsInfo, IEnumerable<PrescriptionHeaderInfo>> query)
        {
            _logger.LogInformation($"Start handling most recents measures : {query}");
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (var uow = _uowFactory.New())
            {
                _logger.LogTrace($"Start querying most recents measures {query}");
                GetMostRecentPrescriptionsInfo input = query.Data;
                Expression<Func<Objects.Prescription, PrescriptionHeaderInfo>> selector = _expressionBuilder.CreateMapExpression<Objects.Prescription, PrescriptionHeaderInfo>();
                IPagedResult<PrescriptionHeaderInfo> items = await uow.Repository<Objects.Prescription>()
                    .ReadPageAsync(
                        selector, 
                        input.Count.GetValueOrDefault(15), 1,
                        new[] { OrderClause<PrescriptionHeaderInfo>.Create(x => x.DeliveryDate, Descending)});

                _logger.LogTrace($"Nb of results : {items.Entries.Count()}");
                _logger.LogInformation($"Handling query {query.Id} successfully");
                return items.Entries;
            }
        }
    }
}
