using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{

    /// <summary>
    /// Data to provide when deleting one <see cref="PhysiologicalMeasurementInfo"/> on a patient
    /// </summary>
    [JsonObject]
    public class DeletePhysiologicalMeasureInfo
    {
        /// <summary>
        /// Id of the patient the measure must be delete from
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>   
        /// Id of the measure to delete
        /// </summary>
        public Guid MeasureId { get; set; }

    }
}
