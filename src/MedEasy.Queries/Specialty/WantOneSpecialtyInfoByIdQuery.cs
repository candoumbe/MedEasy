using System;

namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// Immutable class to query <see cref="DTO.SpecialtyInfo"/> by specifying its <see cref="DTO.SpecialtyInfo.Id"/>
    /// </summary>
    public class WantOneSpecialtyInfoByIdQuery : IWantOneSpecialtyInfoByIdQuery
    {
        public Guid Id { get; }

        public int Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantOneSpecialtyInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="id">of the <see cref="SpecialtyInfo"/> to retrieve</param>
        public WantOneSpecialtyInfoByIdQuery(int id)
        {
            Id = Guid.NewGuid();
            Data = id;
        }

        
    }
}