using System;
using System.Runtime.Serialization;

namespace MedEasy.DTO
{

    [DataContract(Name = "specialty")]
    public class SpecialtyInfo : IBrowsable
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "last_modified")]
        public DateTime? UpdatedDate { get; set; }

        [DataMember(Name = "href")]
        public string Href { get; set; }
    }
}