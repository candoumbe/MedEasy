﻿using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// Argument to used to get doctors by their main specialty id
    /// </summary>
    public class FindDoctorsBySpecialtyIdQueryArgs
    {
        /// <summary>
        /// Id of the specialty
        /// </summary>
        public int SpecialtyId { get; }

        /// <summary>
        /// Page of result configuration
        /// </summary>
        /// <remarks>
        /// This allows to customize the page of result (number of items per page, page size)
        /// </remarks>
        /// <see cref="GenericGetQuery"/>
        public GenericGetQuery GetQuery { get;  }

        /// <summary>
        /// Builds a new <see cref="FindDoctorsBySpecialtyIdQueryArgs"/>
        /// </summary>
        /// <param name="specialtyId">id of the specialty</param>
        /// <param name="getQuery">Configuration of page of result</param>
        public FindDoctorsBySpecialtyIdQueryArgs(int specialtyId, GenericGetQuery getQuery)
        {
            SpecialtyId = specialtyId;
            GetQuery = getQuery;
        }
    }
}