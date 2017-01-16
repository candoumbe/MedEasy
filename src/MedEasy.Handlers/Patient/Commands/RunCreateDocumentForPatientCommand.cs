using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using AutoMapper.QueryableExtensions;
using MedEasy.Commands.Patient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Objects;
using System.Linq.Expressions;
using MedEasy.Handlers.Core.Patient.Commands;
using MedEasy.Handlers.Core.Commands;

namespace MedEasy.Handlers.Patient.Commands
{

    /// <summary>
    /// An instance of this class process <see cref="ICreateDocumentForPatientCommand"/> commands
    /// </summary>
    public class RunCreateDocumentForPatientCommand : CommandRunnerBase<Guid, CreateDocumentForPatientInfo, DocumentMetadataInfo, ICreateDocumentForPatientCommand>, IRunCreateDocumentForPatientCommand
    {
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IUnitOfWorkFactory _factory;
        private readonly ILogger<RunCreateDocumentForPatientCommand> _logger;

        /// <summary>
        /// Builds a new <see cref="RunCreateDocumentForPatientCommand"/> instance.
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="expressionBuilder">Builder that can provide expressions to convert from one type to an other</param>
        /// <param name="validator">Validator to use to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        /// <see cref="CommandRunnerBase{TKey, TInput, TOutput, TCommand}"/>
        public RunCreateDocumentForPatientCommand(IValidate<ICreateDocumentForPatientCommand> validator, ILogger<RunCreateDocumentForPatientCommand> logger, IUnitOfWorkFactory factory,
            IExpressionBuilder expressionBuilder) 
            : base (validator)
        {
            _logger = logger;
            _factory = factory;
            _expressionBuilder = expressionBuilder;
        }


        public override async Task<DocumentMetadataInfo> RunAsync(ICreateDocumentForPatientCommand command)
        {
            _logger.LogInformation($"Start running command : {command}");

            IEnumerable<Task<ErrorInfo>> errorTasks = Validator.Validate(command);
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(errorTasks);

            if (errors.Any(x => x.Severity == ErrorLevel.Error))
            {
                _logger.LogInformation($"Command <{command.Id}> is not valid");

#if DEBUG
                foreach (var error in errors)
                {
                    _logger.LogDebug($"{error.Key} - {error.Severity} : {error.Description}");
                }
#endif

                throw new CommandNotValidException<Guid>(command.Id, errors);
            }

            using (var uow = _factory.New())
            {
                var data = command.Data;
                Objects.Patient patient = await uow.Repository<Objects.Patient>().SingleOrDefaultAsync(x => x.Id == data.PatientId);
                if (patient == null)
                {
                    throw new NotFoundException("No patient found");
                }

                CreateDocumentInfo document = data.Document;
                DocumentMetadata documentMetadata = new DocumentMetadata
                {
                    PatientId = patient.Id,
                    MimeType = document.MimeType,
                    Title = document.Title,
                    Size = document.Content.Length,
                    Document = new Document
                    {
                        Content = data.Document.Content
                    }
                };

                documentMetadata = uow.Repository<DocumentMetadata>().Create(documentMetadata);
                await uow.SaveChangesAsync();

                Expression<Func<DocumentMetadata, DocumentMetadataInfo>> converter = _expressionBuilder.CreateMapExpression<DocumentMetadata, DocumentMetadataInfo>();
                DocumentMetadataInfo result = converter.Compile()(documentMetadata); 

                _logger.LogTrace($"Command's result : {result}");
                _logger.LogInformation($"Command <{command.Id}> runned successfully");

                return result;
            }
        }
    }
}
