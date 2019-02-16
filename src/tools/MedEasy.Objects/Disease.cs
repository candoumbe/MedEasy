namespace MedEasy.Objects
{
    /// <summary>
    /// A disease as defined in the (see  ICD 10
    /// </summary>
    public class Disease : AuditableEntity<int,  Disease>
    {
        public string Code { get; set; }
    }
}
