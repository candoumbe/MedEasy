using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Prescriptions
{
    /// <summary>
    /// Immutable class to query <see cref="DTO.PatientInfo"/> by specifying its <see cref="DTO.PatientInfo.Id"/>
    /// </summary>
    public class WantMostRecentPrescriptionsQuery : IWantMostRecentPrescriptionsQuery
    {
        public Guid Id { get; }

        public GetMostRecentPrescriptionsInfo Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantMostRecentPrescriptionsQuery"/> instance
        /// </summary>
        /// <param name="id">unique id of the <see cref="PatientInfo"/> to retrieve temperature measure from</param>
        /// <param name="measureId">unique id of the measure to retrieve</param>
        public WantMostRecentPrescriptionsQuery(GetMostRecentPrescriptionsInfo input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            Id = Guid.NewGuid();
            Data = input;
        }

        
    }
}