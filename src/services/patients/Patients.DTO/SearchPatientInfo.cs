﻿using MedEasy.DTO.Search;

using System;

namespace Patients.DTO
{
    /// <summary>
    /// Wraps search criteria for <see cref="PatientInfo"/> resources.
    /// </summary>
    public class SearchPatientInfo : AbstractSearchInfo<PatientInfo>
    {
        /// <summary>
        /// Searched <see cref="Firstname"/>
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Searched <see cref="Lastname"/>
        /// </summary>
        public string Lastname { get; set; }

        /// <summary>
        /// Searched <see cref="BirthDate"/>
        /// </summary>
        public DateTime? BirthDate { get; set; }
    }
}
