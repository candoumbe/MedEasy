namespace MedEasy.Measures.API.Controllers
{
    /// <summary>
    /// Wrapper for API settings
    /// </summary>
    public class PrescriptionApiOptions
    {
        /// <summary>
        /// Default page size when reading resource
        /// </summary>
        public int DefaultPageSize { get; set; }

        /// <summary>
        /// Number of resources a page can hold
        /// </summary>
        public int MaxPageSize { get; set; }
    }
}