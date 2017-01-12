using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Objects
{
    /// <summary>
    /// A document submitted by a owner
    /// </summary>
    public class PatientDocument : AuditableEntity<int, PatientDocument>
    {
        /// <summary>
        /// Id of the patient for which the docuemnt is submitted
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Name of the document
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Additional notes on the document
        /// </summary>
        public string Notes { get; set; }

    }
}
