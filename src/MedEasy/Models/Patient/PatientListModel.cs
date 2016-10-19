using System;

namespace MedEasy.ViewModels.Patient
{
    public class PatientListModel : ModelBase<int>
    {
        
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public DateTimeOffset? BirthDate { get; set; }

        public string BirthPlace { get; set; }
    }
}
