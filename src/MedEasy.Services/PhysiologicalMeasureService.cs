using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Queries;
using MedEasy.Objects;
using MedEasy.Validators;
using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands;
using System.Linq;
using MedEasy.Handlers.Exceptions;
using static MedEasy.Validators.ErrorLevel;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;

namespace MedEasy.Services
{
    /// <summary>
    /// Handles everything related to <see cref="PhysiologicalMeasurementInfo"/>
    /// </summary>
    public class PhysiologicalMeasureService : IPhysiologicalMeasureService
    {
        private IUnitOfWorkFactory _uowFactory;
        private readonly ILogger<PhysiologicalMeasureService> _logger;
        private readonly IValidate<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>> _deleteOnePhysiologicalMeasureCommandValidator;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="PhysiologicalMeasureService"/> instance
        /// </summary>
        /// <param name="uowFactory">instance that can create <see cref="IUnitOfWork"/> instances to persist entities</param>
        /// <param name="deleteOnePhysiologicalMeasureCommandValidator">Validates commands</param>
        public PhysiologicalMeasureService(
            IUnitOfWorkFactory uowFactory,
            ILogger<PhysiologicalMeasureService> logger,
            IValidate<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>> deleteOnePhysiologicalMeasureCommandValidator,
            IExpressionBuilder expressionBuilder
            )
        {

            if (uowFactory == null)
            {
                throw new ArgumentNullException(nameof(uowFactory));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (deleteOnePhysiologicalMeasureCommandValidator == null)
            {
                throw new ArgumentNullException(nameof(deleteOnePhysiologicalMeasureCommandValidator));
            }
            if (expressionBuilder == null)
            {
                throw new ArgumentNullException(nameof(expressionBuilder));
            }

            _uowFactory = uowFactory;
            _logger = logger;
            _deleteOnePhysiologicalMeasureCommandValidator = deleteOnePhysiologicalMeasureCommandValidator;
            _expressionBuilder = expressionBuilder;
        }

        /// <summary>
        /// Asynchronously gets the most recent <see cref="BloodPressureInfo"/> measures.
        /// </summary>
        /// <param name="query">specifies which patient to get its most recent measures for</param>
        /// <returns><see cref="IEnumerable{T}"/>holding the most recent <see cref="BloodPressureInfo"/></returns>
        public async Task<IEnumerable<TPhysiologicalMeasureInfo>> GetMostRecentMeasuresAsync<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TPhysiologicalMeasureInfo>> query)
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            where TPhysiologicalMeasureInfo : PhysiologicalMeasurementInfo
        {

            _logger.LogInformation("Querying most recent measures");

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (var uow = _uowFactory.New())
            {
                Expression<Func<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>> selector = _expressionBuilder.CreateMapExpression<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>();
                IPagedResult<TPhysiologicalMeasureInfo> measures = await uow.Repository<TPhysiologicalMeasure>()
                    .WhereAsync(
                        selector,
                        x => x.PatientId == query.Data.PatientId,
                        new[] { OrderClause<TPhysiologicalMeasureInfo>.Create(x => x.DateOfMeasure, SortDirection.Descending) },
                        query.Data.Count.GetValueOrDefault(20),
                        1
                    );
                _logger.LogInformation($"Found {measures.Entries.Count()} results");
                return measures.Entries;
            }
        }

        public async Task<TPhysiologicalMeasureInfo> AddNewMeasureAsync<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(ICommand<Guid, TPhysiologicalMeasure> command)
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            where TPhysiologicalMeasureInfo : PhysiologicalMeasurementInfo
        {
            _logger.LogInformation($"Start adding new measure");
            

            if (command == null)
            {
                _logger.LogError("Command is null");
                throw new ArgumentNullException(nameof(command));
            }


            using (var uow = _uowFactory.New())
            {
                TPhysiologicalMeasure input = command.Data;
                input = uow.Repository<TPhysiologicalMeasure>().Create(input);
                await uow.SaveChangesAsync().ConfigureAwait(false);

                return _expressionBuilder.CreateMapExpression<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>().Compile()(input);
            }
        }

        public async Task<TPhysiologicalMesureInfo> GetOneMeasureAsync<TPhysiologicalMeasure, TPhysiologicalMesureInfo>(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TPhysiologicalMesureInfo> query)
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            where TPhysiologicalMesureInfo : PhysiologicalMeasurementInfo
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            _logger.LogInformation($"Start querying one measure : {query}");
            using (var uow = _uowFactory.New())
            {
                Expression<Func<TPhysiologicalMeasure, TPhysiologicalMesureInfo>> selector = _expressionBuilder.CreateMapExpression<TPhysiologicalMeasure, TPhysiologicalMesureInfo>();
                TPhysiologicalMesureInfo measure = await uow.Repository<TPhysiologicalMeasure>()
                    .SingleOrDefaultAsync(selector, x => x.PatientId == query.Data.PatientId && x.Id == query.Data.MeasureId);

                _logger.LogInformation($"Measure {(measure == null ? "not" : string.Empty)}found");
                return measure;
            }
        }

        public async Task DeleteOnePhysiologicalMeasureAsync<TPhysiologicalMeasure>(IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo> command) where TPhysiologicalMeasure : PhysiologicalMeasurement
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            _logger.LogInformation($"Start running delete one measure id : {command}");
            IEnumerable<Task<ErrorInfo>> validationTasks = _deleteOnePhysiologicalMeasureCommandValidator.Validate(command);
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(validationTasks).ConfigureAwait(false);

            if (errors.Any(x => x.Severity == Error))
            {
                _logger.LogDebug("Command's validation failed");
#if TRACE || DEBUG

                foreach (ErrorInfo error in errors)
                {
                    _logger.LogTrace($"Validation error : {error.Key} : {error.Description}");
                }
#endif
                throw new CommandNotValidException<Guid>(command.Id, errors);
            }

            using (var uow = _uowFactory.New())
            {
                uow.Repository<TPhysiologicalMeasure>().Delete(x => x.PatientId == command.Data.Id && x.Id == command.Data.MeasureId);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }

            _logger.LogInformation("Measure deleted successfully");

        }
    }
}
