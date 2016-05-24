namespace MedEasy.Objects
{
    public abstract class PrescriptionItem : AuditableEntity<int, PrescriptionItem>
    {
        public PrescriptionItemCategory Category { get; set; }

        public string Code { get; set; }
    }
}