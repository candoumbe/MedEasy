namespace Measures.API.Features.Patients
{
    /// <summary>
    /// Model to create a new <see cref="DTO.PatientInfo"/> resource
    /// </summary>
    public class NewPatientModel
    {
        /// <summary>
        /// Name of the patient
        /// </summary>
        public string Name { get; set; }
    }
}
