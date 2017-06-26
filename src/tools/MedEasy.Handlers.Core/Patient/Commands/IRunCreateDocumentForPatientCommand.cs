using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Exceptions;
using Optional;
using System;

namespace MedEasy.Handlers.Core.Patient.Commands
{
    public interface IRunCreateDocumentForPatientCommand : IRunCommandAsync<Guid, CreateDocumentForPatientInfo, Option<DocumentMetadataInfo, CommandException>, ICreateDocumentForPatientCommand>
    {

    }

}
