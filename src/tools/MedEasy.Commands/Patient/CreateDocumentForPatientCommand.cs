using System;
using MedEasy.DTO;
using Newtonsoft.Json;

namespace MedEasy.Commands.Patient
{
    /// <summary>
    /// Command to create a new <see cref="DocumentMetadataInfo"/> resource for a <see cref="PatientInfo"/>.
    /// </summary>
    [JsonObject]
    public class CreateDocumentForPatientCommand : CommandBase<Guid, CreateDocumentForPatientInfo, DocumentMetadataInfo>, ICreateDocumentForPatientCommand
    {
        /// <summary>
        /// Builds a new <see cref="CreatePatientCommand"/> instance.
        /// </summary>
        /// <param name="patientId">id of the patient the <paramref name="documentInfo"/> will be attached to.</param>
        /// <param name="documentInfo">The document to attach.</param>
        public CreateDocumentForPatientCommand(Guid patientId, CreateDocumentInfo documentInfo) : this(new CreateDocumentForPatientInfo { Document = documentInfo, PatientId = patientId })
        {

        }

        /// <summary>
        /// Builds a new <see cref="CreatePatientCommand"/> instance.
        /// </summary>
        /// <param name="data">data that will be used to create the patient resource</param>
        /// <see cref="CommandBase{TKey, TData}"/>
        /// <see cref="ICreateDocumentForPatientCommand"/>
        /// <see cref="CreateDocumentForPatientInfo"/>
        public CreateDocumentForPatientCommand(CreateDocumentForPatientInfo data) : base(Guid.NewGuid(), data)
        {
            
        }
    }


    
}
