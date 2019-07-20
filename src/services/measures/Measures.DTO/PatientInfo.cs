﻿using MedEasy.RestObjects;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Measures.DTO
{
    /// <summary>
    /// Informations on a patient
    /// </summary>
    [DataContract]
    public class PatientInfo : Resource<Guid>
    {
        [DataMember(Name = nameof(Name))]
        public string Name { get; set; }

        /// <summary>
        /// Patient's birth date
        /// </summary>
        public DateTime? BirthDate { get; set; }
    }
}