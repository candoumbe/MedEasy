using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{
    [DataContract]
    public class BrowsableDoctorInfo : IBrowsable
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Firstname { get; set; }

        [DataMember]
        public string Lastname { get; set; }

        [DataMember]
        public SpecialtyInfo Specialty { get; set; }

        [DataMember]
        public DateTime? UpdatedDate { get; set; }

        [DataMember]
        public int? SpecialtyId { get; set; }
        [DataMember]
        public string Href { get; set; }
    }

}
