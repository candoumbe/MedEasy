using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Patient
{
    public interface IWantPatientInfoByIdQuery : IQuery<Guid, int, PatientInfo>
    {
        
    }
}
