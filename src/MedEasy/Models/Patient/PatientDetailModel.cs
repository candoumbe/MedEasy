using System;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.ViewModels.Patient
{
    public class PatientDetailModel : ModelBase<int>
    {

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(50)]
        public string BirthPlace { get; set; }


    }
}
