using System;
using MedEasy.RestObjects;
using MedEasy.DTO;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Immutable class to query many <see cref="DocumentInfo"/>s by specifying its <see cref="PatientInfo.Id"/>
    /// </summary>
    public sealed class WantDocumentsByPatientIdQuery : IWantDocumentsByPatientIdQuery
    {
        public Guid Id { get; }

        public GetDocumentsByPatientIdInfo Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantDocumentsByPatientIdQuery"/> instance.
        /// </summary>
        /// <param name="patientId">id of the patient</param>
        /// <param name="pageConfiguration">Configuration used to split results in pages.</param>
        public WantDocumentsByPatientIdQuery(int patientId, GenericGetQuery pageConfiguration)
        {
            Id = Guid.NewGuid();
            Data = new GetDocumentsByPatientIdInfo { PatientId = patientId, PageConfiguration = pageConfiguration };
        }

        
    }
}