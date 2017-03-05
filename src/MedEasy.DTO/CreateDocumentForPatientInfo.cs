using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace MedEasy.DTO
{
    /// <summary>
    /// Defines the data of a command to create a <see cref="DocumentMetadataInfo"/> for a <see cref="PatientInfo"/>.
    /// </summary>
    [JsonObject]
    public class CreateDocumentForPatientInfo
    {
        /// <summary>
        /// Id of the patient.
        /// </summary>
        /// <remarks>
        /// This is the id of the <see cref="PatientInfo"/> the <see cref="DocumentMetadataInfo"/> will be 
        /// attached to.
        /// </remarks>
        public Guid PatientId { get; set; }


        public CreateDocumentInfo Document { get; set; }
    }
}