using System;
using MedEasy.DTO;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Immutable class to query one <see cref="DocumentInfo"/>s by specifying the patient id and the document id
    /// </summary>
    public sealed class WantOneDocumentByPatientIdAndDocumentIdQuery : IWantOneDocumentByPatientIdAndDocumentIdQuery
    {
        public Guid Id { get; }

        public GetOneDocumentInfoByPatientIdAndDocumentIdInfo Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantDocumentsByPatientIdQuery"/> instance.
        /// </summary>
        /// <param name="patientId">id of the patient</param>
        /// <param name="documentMetadataId">Configuration used to split results in pages.</param>
        public WantOneDocumentByPatientIdAndDocumentIdQuery(Guid patientId, Guid documentMetadataId)
        {
            Id = Guid.NewGuid();
            Data = new GetOneDocumentInfoByPatientIdAndDocumentIdInfo { PatientId = patientId, DocumentMetadataId = documentMetadataId };
        }

        
    }
}