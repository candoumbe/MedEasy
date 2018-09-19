namespace Measures.API.Features.Patients
{
    /// <summary>
    /// Model to create a new <see cref="DTO.PatientInfo"/> resource
    /// </summary>
    public class NewPatientModel
    {
        /// <summary>
        /// Patient's firstname
        /// </summary>
        public string Firstname { get; set; }


        /// <summary>
        /// Patient's lastname
        /// </summary>
        public string Lastname { get; set; }
    }
}
