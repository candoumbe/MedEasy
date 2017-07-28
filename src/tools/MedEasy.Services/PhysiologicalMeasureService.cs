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
using MedEasy.Commands;
using System.Linq;
using MedEasy.Handlers.Core.Exceptions;
using static MedEasy.Validators.ErrorLevel;
using System.Linq.Expressions;
using MedEasy.DAL.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Threading;
using Optional;
using MedEasy.Queries.Patient;

namespace MedEasy.Services
{
    /// <summary>
    /// Handles everything related to <see cref="PhysiologicalMeasurementInfo"/>
    /// </summary>
    public class PhysiologicalMeasureService : IPhysiologicalMeasureService
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly ILogger<PhysiologicalMeasureService> _logger;
        private readonly IValidate<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>> _deleteOnePhysiologicalMeasureCommandValidator;
        private readonly IMapper _mapper;

        /// <summary>
        /// Builds a new <see cref="PhysiologicalMeasureService"/> instance
        /// </summary>
        /// <param name="uowFactory">instance that can create <see cref="IUnitOfWork"/> instances to persist entities</param>
        /// <param name="deleteOnePhysiologicalMeasureCommandValidator">Validates commands</param>
        public PhysiologicalMeasureService(
            IUnitOfWorkFactory uowFactory,
            ILogger<PhysiologicalMeasureService> logger,
            IValidate<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>> deleteOnePhysiologicalMeasureCommandValidator,
            IMapper mapper
            )
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _deleteOnePhysiologicalMeasureCommandValidator = deleteOnePhysiologicalMeasureCommandValidator ?? throw new ArgumentNullException(nameof(deleteOnePhysiologicalMeasureCommandValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Asynchronously gets the most recent <see cref="TPhysiologicalMeasureInfo"/> measures.
        /// </summary>
        /// <param name="query">specifies which patient to get its most recent measures for</param>
        /// <returns><see cref="IEnumerable{TPhysiologicalMeasureInfo}"/>holding the most recent <see cref="TPhysiologicalMeasureInfo"/></returns>
        public async ValueTask<Option<IEnumerable<TPhysiologicalMeasureInfo>>> GetMostRecentMeasuresAsync<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(IWantMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasureInfo> query, CancellationToken cancellationToken = default(CancellationToken))
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            where TPhysiologicalMeasureInfo : PhysiologicalMeasurementInfo
        {

            _logger.LogInformation("Querying most recent measures");

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (IUnitOfWork uow = _uowFactory.New())
            {
                Option<IEnumerable<TPhysiologicalMeasureInfo>> result;
                Expression<Func<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>> selector = _mapper.ConfigurationProvider.ExpressionBuilder.GetMapExpression<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>();
                if (await uow.Repository<Patient>().AnyAsync(x => x.UUID == query.Data.PatientId).ConfigureAwait(false))
                {
                    IPagedResult<TPhysiologicalMeasureInfo> measures = await uow.Repository<TPhysiologicalMeasure>()
                                .WhereAsync(
                                    selector,
                                    (TPhysiologicalMeasure x) => x.Patient.UUID == query.Data.PatientId,
                                    new[] { OrderClause<TPhysiologicalMeasureInfo>.Create(x => x.DateOfMeasure, SortDirection.Descending) },
                                    query.Data.Count.GetValueOrDefault(20),
                                    1,
                                    cancellationToken
                                )
                                .ConfigureAwait(false);
                    int nbResults = measures.Entries.Count();
                    result = measures.Entries.Some();
                    _logger.LogInformation($"Found {measures.Entries.Count()} result{(nbResults > 1 ? "s" : string.Empty)}"); 
                }
                else
                {
                    result = Option.None<IEnumerable<TPhysiologicalMeasureInfo>>();
                }
                return result;
            }
        }

        public async ValueTask<Option<TPhysiologicalMeasureInfo, CommandException>> AddNewMeasureAsync<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(ICommand<Guid, CreatePhysiologicalMeasureInfo<TPhysiologicalMeasure>, TPhysiologicalMeasureInfo> command, CancellationToken cancellationToken = default(CancellationToken))
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            where TPhysiologicalMeasureInfo : PhysiologicalMeasurementInfo
        {
            
            _logger.LogInformation($"Start adding new measure");

            if (command == null)
            {
                _logger.LogError("Command is null");
                throw new ArgumentNullException(nameof(command));
            }

            Option<TPhysiologicalMeasureInfo, CommandException> result;
            using (IUnitOfWork uow = _uowFactory.New())
            {
                if (!await uow.Repository<Patient>().AnyAsync(x => x.UUID == command.Data.PatientId, cancellationToken).ConfigureAwait(false))
                {
                    result = Option.None<TPhysiologicalMeasureInfo, CommandException>(new CommandEntityNotFoundException($"Patient <{command.Data.PatientId}> not found"));
                }
                else
                {

                    CreatePhysiologicalMeasureInfo<TPhysiologicalMeasure> input = command.Data;
                    TPhysiologicalMeasure newMeasure = input.Measure;
                    newMeasure = uow.Repository<TPhysiologicalMeasure>().Create(newMeasure);
                    await uow.SaveChangesAsync(cancellationToken)
                            .ConfigureAwait(false);
                    TPhysiologicalMeasureInfo output = _mapper.Map<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(newMeasure);
                    output.PatientId = input.PatientId;

                    result = output.Some<TPhysiologicalMeasureInfo, CommandException>();
                    _logger.LogInformation("Command <{0}> completed successfully", command.Id);

                }

                return result;
            }
        }

        public async ValueTask<Option<TPhysiologicalMesureInfo>> GetOneMeasureAsync<TPhysiologicalMeasure, TPhysiologicalMesureInfo>(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TPhysiologicalMesureInfo> query, CancellationToken cancellationToken = default(CancellationToken))
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            where TPhysiologicalMesureInfo : PhysiologicalMeasurementInfo
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            _logger.LogInformation($"Start querying one measure : {query}");

            using (IUnitOfWork uow = _uowFactory.New())
            {
                Expression<Func<TPhysiologicalMeasure, TPhysiologicalMesureInfo>> selector = _mapper.ConfigurationProvider.ExpressionBuilder.GetMapExpression<TPhysiologicalMeasure, TPhysiologicalMesureInfo>();
                Option<TPhysiologicalMesureInfo> measure = await uow.Repository<TPhysiologicalMeasure>()
                    .SingleOrDefaultAsync(selector, x => x.Patient.UUID == query.Data.PatientId && x.UUID == query.Data.MeasureId)
                    .ConfigureAwait(false);

                measure.Match(
                    some: (_) => _logger.LogInformation($"Measure found"),
                    none: () => _logger.LogInformation($"Measure not found")
                    );


                return measure;
            }
        }

        public async Task DeleteOnePhysiologicalMeasureAsync<TPhysiologicalMeasure>(IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo> command, CancellationToken cancellationToken = default(CancellationToken)) where TPhysiologicalMeasure : PhysiologicalMeasurement
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            _logger.LogInformation($"Start running delete one measure id : {command}");
            IEnumerable<Task<ErrorInfo>> validationTasks = _deleteOnePhysiologicalMeasureCommandValidator.Validate(command);
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(validationTasks);

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

            using (IUnitOfWork uow = _uowFactory.New())
            {
                uow.Repository<TPhysiologicalMeasure>().Delete(x => x.Patient.UUID == command.Data.Id && x.UUID == command.Data.MeasureId);
                await uow.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            _logger.LogInformation("Measure deleted successfully");

        }
    }
}
