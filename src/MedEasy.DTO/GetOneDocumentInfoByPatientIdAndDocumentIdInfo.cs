using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Query to get <see cref="DocumentInfo"/>s for a <see cref="PatientInfo"/>
    /// </summary>
    public class GetOneDocumentInfoByPatientIdAndDocumentIdInfo
    {
        /// <summary>
        /// Id of the patient 
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Id of the document metadata to get file from
        /// </summary>
        public Guid DocumentMetadataId { get; set; }

    }
}
