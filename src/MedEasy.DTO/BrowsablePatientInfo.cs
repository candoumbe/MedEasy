using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{
    [JsonObject]
    public class BrowsablePatientInfo : BrowsableResource<PatientInfo>
    {
        
    }
}
