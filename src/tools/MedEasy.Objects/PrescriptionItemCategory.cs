using System;

namespace MedEasy.Objects
{
    /// <summary>
    /// An instance of this type defines a category of prescriptions' item
    /// </summary>
    public class PrescriptionItemCategory: AuditableEntity<int, PrescriptionItemCategory>
    {
        public string Code { get; set; }

        public PrescriptionItemCategory(Guid uuid): base(uuid)
        {

        }
    }
}