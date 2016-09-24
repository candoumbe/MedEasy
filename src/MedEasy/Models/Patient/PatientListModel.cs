using System;

namespace MedEasy.ViewModels.Patient
{
    public class PatientListModel : ModelBase<int>
    {
        
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public DateTime? BirthDate { get; set; }

        public string BirthPlace { get; set; }
    }
}
