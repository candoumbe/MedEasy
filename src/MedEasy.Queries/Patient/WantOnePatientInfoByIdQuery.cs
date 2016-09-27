using System;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Immutable class to query <see cref="DTO.PatientInfo"/> by specifying its <see cref="DTO.PatientInfo.Id"/>
    /// </summary>
    public class WantOnePatientInfoByIdQuery : IWantOnePatientInfoByIdQuery
    {
        public Guid Id { get; }

        public int Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantOnePatientInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="id">of the <see cref="PatientInfo"/> to retrieve</param>
        public WantOnePatientInfoByIdQuery(int id)
        {
            Id = Guid.NewGuid();
            Data = id;
        }

        
    }
}