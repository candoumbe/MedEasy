using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.ViewModels.Doctor
{
    public class DoctorCreateModel : ModelBase<int>
    {
        
        [StringLength(255)]
        public string Firstname { get; set; }

        [StringLength(255)]
        public string Lastname { get; set; }

        public IList<int?> Specialties { get; set; }
    }
}
