using Newtonsoft.Json;
using static Newtonsoft.Json.JsonConvert;
namespace MedEasy.DTO
{

    /// <summary>
    /// Data to update a <see cref="PatientInfo.MainDoctorId"/>
    /// </summary>
    [JsonObject]
    public class ChangeMainDoctorIdInfo
    {
        /// <summary>
        /// Id of the patient the change should be made on
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// New doctor's
        /// </summary>
        public int? NewDoctorId { get; set; }


        public override string ToString() => SerializeObject(this);
    }
}
