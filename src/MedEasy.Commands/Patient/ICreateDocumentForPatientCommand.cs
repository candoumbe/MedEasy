using System;
using MedEasy.DTO;

namespace MedEasy.Commands.Patient
{
    public interface ICreateDocumentForPatientCommand : ICommand<Guid, CreateDocumentForPatientInfo>
    {    }
}