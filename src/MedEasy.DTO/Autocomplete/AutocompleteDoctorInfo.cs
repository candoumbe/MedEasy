using MedEasy.DTO.Autocomplete;
using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO.Autocomplete
{
    [DataContract]
    public class DoctorAutocompleteInfo : AutocompleteInfo<int>
    {
        [DataMember]
        public string Firstname { get; set; }

        [DataMember]
        public string Lastname { get; set; }

        [DataMember]
        public string Specialty { get; set; }
        
    }

}
