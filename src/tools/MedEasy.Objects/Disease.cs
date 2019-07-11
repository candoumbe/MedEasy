using System;

namespace MedEasy.Objects
{
    /// <summary>
    /// A disease as defined in the (see  ICD 10
    /// </summary>
    public class Disease : AuditableEntity<Guid,  Disease>
    {
        public string Code { get; set; }

        public Disease(Guid id) : base(id)
        {

        }
    }
}
