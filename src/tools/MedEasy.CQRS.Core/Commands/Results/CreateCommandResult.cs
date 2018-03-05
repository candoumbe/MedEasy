using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.CQRS.Core.Commands.Results
{
    public enum CreateCommandResult
    {
        /// <summary>
        /// The command complete successfully
        /// </summary>
        Done,
        /// <summary>
        /// Command failed because of a (potential) conflict
        /// </summary>
        Failed_Conflict,
        /// <summary>
        /// Command failed because of a (related) resource not found
        /// </summary>
        Failed_NotFound,
        /// <summary>
        /// Command failed because of restriction
        /// </summary>
        Failed_Unauthorized
    }
}
