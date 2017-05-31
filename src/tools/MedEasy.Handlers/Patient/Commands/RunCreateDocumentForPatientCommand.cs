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
using AutoMapper;
using System.Threading;

namespace MedEasy.Handlers.Patient.Commands
{

    /// <summary>
    /// An instance of this class process <see cref="ICreateDocumentForPatientCommand"/> commands
    /// </summary>
    public class RunCreateDocumentForPatientCommand : CommandRunnerBase<Guid, CreateDocumentForPatientInfo, DocumentMetadataInfo, ICreateDocumentForPatientCommand>, IRunCreateDocumentForPatientCommand
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWorkFactory _factory;
        private readonly ILogger<RunCreateDocumentForPatientCommand> _logger;

        /// <summary>
        /// Builds a new <see cref="RunCreateDocumentForPatientCommand"/> instance.
        /// </summary>
        /// <param name="factory"> Factory that can build<see cref="IUnitOfWorkFactory"/></param>
        /// <param name="mapper">Mapper to convert from one type to an other</param>
        /// <param name="validator">Validator to use to validate commands before processing them</param>
        /// <param name="logger">logger</param>
        /// <exception cref="ArgumentNullException"> if any of the parameters is <c>null</c></exception>
        /// <see cref="CommandRunnerBase{TKey, TInput, TOutput, TCommand}"/>
        public RunCreateDocumentForPatientCommand(IValidate<ICreateDocumentForPatientCommand> validator, ILogger<RunCreateDocumentForPatientCommand> logger, IUnitOfWorkFactory factory,
            IMapper mapper) 
            : base (validator)
        {
            _logger = logger;
            _factory = factory;
            _mapper = mapper;
        }


        public override async Task<DocumentMetadataInfo> RunAsync(ICreateDocumentForPatientCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation($"Start running command : {command}");

            IEnumerable<Task<ErrorInfo>> errorTasks = Validator.Validate(command);
            IEnumerable<ErrorInfo> errors = await Task.WhenAll(errorTasks);

            if (errors.Any(x => x.Severity == ErrorLevel.Error))
            {
                _logger.LogInformation($"Command <{command.Id}> is not valid");

#if DEBUG
                foreach (ErrorInfo error in errors)
                {
                    _logger.LogDebug($"{error.Key} - {error.Severity} : {error.Description}");
                }
#endif

                throw new CommandNotValidException<Guid>(command.Id, errors);
            }

            using (IUnitOfWork uow = _factory.New())
            {
                CreateDocumentForPatientInfo data = command.Data;
                var patient = await uow.Repository<Objects.Patient>()
                    .SingleOrDefaultAsync(
                        x => new {x.Id}, 
                        x => x.UUID == data.PatientId, cancellationToken);
                if (patient == null)
                {
                    throw new NotFoundException($"Patient <{data.PatientId}> not found");
                }

                CreateDocumentInfo document = data.Document;
                DocumentMetadata documentMetadata = new DocumentMetadata
                {
                    PatientId = patient.Id,
                    MimeType = document.MimeType,
                    Title = document.Title,
                    Size = document.Content.Length,
                    Document = new Objects.Document
                    {
                        Content = data.Document.Content,
                        UUID = Guid.NewGuid()
                    },
                    UUID = Guid.NewGuid()
                };

                documentMetadata = uow.Repository<DocumentMetadata>().Create(documentMetadata);
                await uow.SaveChangesAsync(cancellationToken);

                DocumentMetadataInfo result = _mapper.Map<DocumentMetadata, DocumentMetadataInfo>(documentMetadata);
                result.PatientId = data.PatientId;
                _logger.LogTrace($"Command's result : {result}");
                _logger.LogInformation($"Command <{command.Id}> runned successfully");

                return result;
            }
        }
    }
}
