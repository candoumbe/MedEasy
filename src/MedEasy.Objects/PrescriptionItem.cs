namespace MedEasy.Objects
{
    public abstract class PrescriptionItem : AuditableEntity<int, PrescriptionItem>
    {
        public PrescriptionItemCategory Category { get; set; }

        /// <summary>
        /// Code of the prescription
        /// </summary>
        public string Code { get; set; }
    }
}