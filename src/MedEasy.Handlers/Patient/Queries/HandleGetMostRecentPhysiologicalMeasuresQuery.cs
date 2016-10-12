﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Queries;
using Microsoft.Extensions.Logging;
using MedEasy.Objects;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using static MedEasy.DAL.Repositories.SortDirection;
using System.Linq;

namespace MedEasy.Handlers.Patient.Queries
{
    public class HandleGetMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasure, TPhysiologicalMeasureInfo> : IHandleGetMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasureInfo>
        where TPhysiologicalMeasure : PhysiologicalMeasurement
        where TPhysiologicalMeasureInfo : PhysiologicalMeasurementInfo
    {
        private IExpressionBuilder _expressionBuilder;
        private IUnitOfWorkFactory _uowFactory;
        private ILogger<HandleGetMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>> _logger;


        /// <summary>
        /// Builds a new <see cref="HandleGetMostRecentPhysiologicalMeasuresQuery{TPhysiologicalMeasure}"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="logger"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetMostRecentPhysiologicalMeasuresQuery(IUnitOfWorkFactory uowFactory, ILogger<HandleGetMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>> logger, IExpressionBuilder expressionBuilder)
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

        public async Task<IEnumerable<TPhysiologicalMeasureInfo>> HandleAsync(IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TPhysiologicalMeasureInfo>> query)
        {
            _logger.LogInformation($"Start handling most recents measures : {query}");
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (var uow = _uowFactory.New())
            {
                _logger.LogTrace($"Start querying most recents measures {query}");
                GetMostRecentPhysiologicalMeasuresInfo input = query.Data;
                Expression<Func<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>> selector = _expressionBuilder.CreateMapExpression<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>();
                IPagedResult<TPhysiologicalMeasureInfo> mostRecentsMeasures = await uow.Repository<TPhysiologicalMeasure>()
                    .WhereAsync(
                        selector,
                        x => x.PatientId == input.Id,
                        new[] { OrderClause<TPhysiologicalMeasureInfo>.Create(x => x.DateOfMeasure, Descending)}, 
                        1, input.Count.GetValueOrDefault(15));

                _logger.LogTrace($"Nb of results : {mostRecentsMeasures.Entries.Count()}");
                _logger.LogInformation($"Handling query {query.Id} successfully");
                return mostRecentsMeasures.Entries;
            }
        }
    }
}
