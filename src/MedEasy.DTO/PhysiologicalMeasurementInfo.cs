using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    [JsonObject]
    public abstract class PhysiologicalMeasurementInfo : IResource<int>
    {
        public int Id { get; set; }

        /// <summary>
        /// Id of the <see cref="PatientInfo"/> resource the measure was taken on
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// When the measure was made
        /// </summary>
        public DateTime DateOfMeasure { get; set; }
    }
}
