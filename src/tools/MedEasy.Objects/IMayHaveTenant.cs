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
        Guid? TenantId { get; }

        /// <summary>
        /// Defines the <see cref="Tenantid"/>
        /// </summary>
        /// <param name="tenantId"></param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tenantId"/> is <see cref="Guid.Empty"/></exception>
        void SetTenant(Guid? tenantId);
    }
}
