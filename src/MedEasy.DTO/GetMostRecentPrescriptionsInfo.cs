using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;
namespace MedEasy.DTO
{
    /// <summary>
    /// Data to provide when querying many physiological measure information
    /// </summary>
    [JsonObject]
    public class GetMostRecentPrescriptionsInfo
    {
        /// <summary>
        /// Id of the patient to get one measure from
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PatientId { get; set; }

        /// <summary>
        /// Number of prescriptions to return
        /// </summary>
        [Range(0, int.MaxValue)]
        public int? Count { get; set; }


        public override string ToString() => SerializeObject(this);
    }
}
