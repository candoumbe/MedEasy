using System;
using MedEasy.DTO;

namespace MedEasy.Commands.Patient
{
    public interface ICreatePatientCommand : ICommand<Guid, CreatePatientInfo, PatientInfo>
    {    }
}