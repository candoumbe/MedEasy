using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.Objects
{
    /// <summary>
    /// Adds multitenancy support
    /// </summary>
    public interface IMayHaveTenant
    {
        Guid? TenantId { get; set; }
    }
}
