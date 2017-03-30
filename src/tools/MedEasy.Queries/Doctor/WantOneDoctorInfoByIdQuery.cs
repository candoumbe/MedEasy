using System;

namespace MedEasy.Queries.Doctor
{
    /// <summary>
    /// Immutable class to query <see cref="DTO.DoctorInfo"/> by specifying its <see cref="DTO.DoctorInfo.Id"/>
    /// </summary>
    public class WantOneDoctorInfoByIdQuery : IWantOneDoctorInfoByIdQuery
    {
        public Guid Id { get; }

        public Guid Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantOneDoctorInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="id">of the <see cref="DoctorInfo"/> to retrieve</param>
        public WantOneDoctorInfoByIdQuery(Guid id)
        {
            Id = Guid.NewGuid();
            Data = id;
        }

        
    }
}