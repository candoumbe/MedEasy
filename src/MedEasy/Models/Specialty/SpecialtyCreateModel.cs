using System.ComponentModel.DataAnnotations;

namespace MedEasy.ViewModels.Specialty
{
    public class SpecialtyCreateModel : ModelBase<int>
    {
        [StringLength(255)]
        public string Code { get; set; }

        [StringLength(255)]
        public string Name { get; set; }
        
    }
}
