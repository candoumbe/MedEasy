using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// Wraps a query to find <see cref="DoctorInfo"/>s by a specialty id 
    /// </summary>
    public class FindDoctorsBySpecialtyIdQuery : IFindDoctorsBySpecialtyIdQuery
    {

        public Guid Id { get; }


        public FindDoctorsBySpecialtyIdQueryArgs Data { get; }

        /// <summary>
        /// Builds a new <see cref="FindDoctorsBySpecialtyIdQuery"/>
        /// </summary>
        /// <param name="args">containts</param>
        public FindDoctorsBySpecialtyIdQuery(Guid specialtyId, PaginationConfiguration query)
        {
            Data = new FindDoctorsBySpecialtyIdQueryArgs(specialtyId, query);
        }
    }
}
