using System;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Immutable class to query <see cref="DTO.PatientInfo"/> by specifying its <see cref="DTO.PatientInfo.Id"/>
    /// </summary>
    public class WantPatientInfoByIdQuery : IWantPatientInfoByIdQuery
    {
        public Guid Id { get; }

        public int Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantPatientInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="id"></param>
        public WantPatientInfoByIdQuery(int id)
        {
            Id = Guid.NewGuid();
            Data = id;
        }

        
    }
}