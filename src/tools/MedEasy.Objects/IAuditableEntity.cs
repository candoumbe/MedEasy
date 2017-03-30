using System;

namespace MedEasy.Objects
{
    public interface IAuditableEntity
    {
        DateTimeOffset? CreatedDate { get; set; }

        string CreatedBy { get; set; }

        DateTimeOffset? UpdatedDate { get; set; }

        string UpdatedBy { get; set; }
    }
}
