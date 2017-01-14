using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Query to get <see cref="DocumentMetadataInfo"/>s for a <see cref="PatientInfo"/>
    /// </summary>
    public class GetDocumentsByPatientIdInfo
    {
        /// <summary>
        /// Id of the patient to get its <see cref="DocumentMetadataInfo"/>s for
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Page configuration
        /// </summary>
        public PaginationConfiguration PageConfiguration { get; set; }

    }
}
