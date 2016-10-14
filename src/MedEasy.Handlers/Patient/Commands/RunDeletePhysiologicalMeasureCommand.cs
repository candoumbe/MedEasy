using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using MedEasy.Objects;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using MedEasy.Commands.Patient;
using System.Collections.Generic;
using System.Linq;
using static MedEasy.Validators.ErrorLevel;
using MedEasy.Handlers.Exceptions;

namespace MedEasy.Handlers.Patient.Commands
{
    /// <summary>
    /// Can be used delete one <see cref="PhysiologicalMeasurement"/> instance.
    /// </summary>
    /// <typeparam name="TKey">Type of the ID of commands this instance can run</typeparam>
    /// <typeparam name="TEntity">Type of the entity this instance will delete</typeparam>
    public class RunDeletePhysiologicalMeasureCommand<TKey, TEntity> : CommandRunnerBase<TKey, DeletePhysiologicalMeasureInfo, IDeleteOnePhysiologicalMeasureCommand<TKey, DeletePhysiologicalMeasureInfo>>, IRunDeletePhysiologicalMeasureCommand<TKey, DeletePhysiologicalMeasureInfo>
        where TKey : IEquatable<TKey>
        where TEntity : PhysiologicalMeasurement
    {
        private readonly ILogger<RunDeletePhysiologicalMeasureCommand<TKey, TEntity>> _logger;
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Builds a new <see cref="RunDeletePhysiologicalMeasureCommand{TEntity, TCommand}"/> instance.
        /// </summary>
        /// <remarks>
        /// This runner can be used to run commands that delete one <see cref="PhysiologicalMeasurement"/> for a specified <see cref="Objects.Patient"/>
        /// </remarks>
        /// <param name="validator">will be used to validate commands in <see cref="RunAsync(TCommand)"/></param>
        /// <param name="logger">logger to track instance usage</param>
        /// <param name="uowFactory"></param>
        public RunDeletePhysiologicalMeasureCommand(IValidate<IDeleteOnePhysiologicalMeasureCommand<TKey, DeletePhysiologicalMeasureInfo>> validator, ILogger<RunDeletePhysiologicalMeasureCommand<TKey, TEntity>> logger, IUnitOfWorkFactory uowFactory) : base(validator)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (uowFactory == null)
            {
                throw new ArgumentNullException(nameof(uowFactory));
            }

            _logger = logger;
            _uowFactory = uowFactory;
        }

        public override async Task RunAsync(IDeleteOnePhysiologicalMeasureCommand<TKey, DeletePhysiologicalMeasureInfo> command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            _logger.LogInformation($"Start running delete one measure id : {command}");
            IEnumerable<Task<ErrorInfo>> validationTasks = Validator.Validate(command);
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
                throw new CommandNotValidException<TKey>(command.Id, errors);
            }

            using (var uow = _uowFactory.New())
            {
                uow.Repository<TEntity>().Delete(x => x.PatientId == command.Data.Id && x.Id == command.Data.MeasureId);
                await uow.SaveChangesAsync().ConfigureAwait(false);
            }

            _logger.LogInformation("Measure deleted successfully");

        }
    }
}
