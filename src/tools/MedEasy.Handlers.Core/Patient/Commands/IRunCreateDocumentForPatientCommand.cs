using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Patient.Commands
{
    public interface IRunCreateDocumentForPatientCommand : IRunCommandAsync<Guid, CreateDocumentForPatientInfo, DocumentMetadataInfo, ICreateDocumentForPatientCommand>
    {

    }

}
