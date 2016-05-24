using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{
    [DataContract]
    public class PatientInfo : IBrowsable
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Firstname { get; set; }

        [DataMember]
        public string Lastname { get; set; }

        [DataMember]
        public DateTime? BirthDate { get; set; }

        [DataMember]
        public string BirthPlace { get; set; }

        [DataMember]
        public BrowsableDoctorInfo MainDoctor { get; set; }

        [DataMember]
        public int? MainDoctorId { get; set; }


        [DataMember]
        public string Fullname { get; set; }

        [DataMember]
        public DateTime? UpdatedDate { get; set; }

        [DataMember]
        public string Href { get; set; }
    }
}
