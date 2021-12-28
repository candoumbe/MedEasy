namespace Measures.API.Features.Subjects
{
    /// <summary>
    /// Model to create a new <see cref="DTO.SubjectInfo"/> resource
    /// </summary>
    public class NewSubjectModel
    {
        /// <summary>
        /// Name of the patient
        /// </summary>
        public string Name { get; set; }
    }
}
