﻿using System;
using MedEasy.RestObjects;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Immutable class to query many <see cref="DTO.PatientInfo"/> by specifying its <see cref="DTO.PatientInfo.Id"/>
    /// </summary>
    public class WantManyPatientInfosQuery : IWantPageOfPatientInfoQuery
    {
        public Guid Id { get; }

        public PaginationConfiguration Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantManyPatientInfosQuery"/> instance
        /// </summary>
        /// <param name="queryConfig">configuration of the query</param>
        public WantManyPatientInfosQuery(PaginationConfiguration queryConfig)
        {
            Id = Guid.NewGuid();
            Data = queryConfig;
        }

        
    }
}