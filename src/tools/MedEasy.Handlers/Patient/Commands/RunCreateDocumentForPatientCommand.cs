using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using MedEasy.Validators;
using Microsoft.Extensions.Logging;
using MedEasy.Commands.Patient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Objects;
using MedEasy.Handlers.Core.Patient.Commands;
using MedEasy.Handlers.Core.Commands;
using AutoMapper;
using System.Threading;
using Optional;

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
            : base(validator)
        {
            _logger = logger;
            _factory = factory;
            _mapper = mapper;
        }


        public override async Task<Option<DocumentMetadataInfo, CommandException>> RunAsync(ICreateDocumentForPatientCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Option<DocumentMetadataInfo, CommandException> result;

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
                result = Option.None<DocumentMetadataInfo, CommandException>(new CommandNotValidException<Guid>(command.Id, errors));
            }
            else
            {
                using (IUnitOfWork uow = _factory.New())
                {
                    CreateDocumentForPatientInfo data = command.Data;
                    var patient = await uow.Repository<Objects.Patient>()
                        .SingleOrDefaultAsync(
                            x => new { x.Id },
                            x => x.UUID == data.PatientId, cancellationToken);
                    int? patientId = null;
                    patient.MatchSome(x => patientId = x.Id);

                    if (!patientId.HasValue)
                    {
                        result = Option.None<DocumentMetadataInfo, CommandException>(new CommandEntityNotFoundException($"Patient <{data.PatientId}> not found"));
                    }
                    else
                    {
                        CreateDocumentInfo document = data.Document;
                        DocumentMetadata documentMetadata = new DocumentMetadata
                        {
                            PatientId = patientId.Value,
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

                        DocumentMetadataInfo documentMetadataInfo = _mapper.Map<DocumentMetadata, DocumentMetadataInfo>(documentMetadata);
                        documentMetadataInfo.PatientId = data.PatientId;

                        result = Option.Some<DocumentMetadataInfo, CommandException>(documentMetadataInfo);

                        _logger.LogTrace($"Command's result : {documentMetadataInfo}");
                        _logger.LogInformation($"Command <{command.Id}> runned successfully");

                    }


                }
            }
            return result;
        }
    }
}
