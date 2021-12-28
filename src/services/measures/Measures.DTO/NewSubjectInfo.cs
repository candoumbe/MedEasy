namespace Measures.DTO
{
    using Measures.Ids;


    using NodaTime;

    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// data to provide when creating a new patient resource
    /// </summary>
    public class NewSubjectInfo
    {
        /// <summary>
        /// Id of the resource to create
        /// </summary>
        public SubjectId Id { get; set; }

        /// <summary>
        /// Patient's firstname
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// Patient's birth date
        /// </summary>
        public LocalDate? BirthDate { get; set; }
    }
}
