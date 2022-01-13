namespace MedEasy.ReverseProxy
{
    /// <summary>
    /// Wrapper for describing a MedEasy REST API
    /// </summary>
    public class MedEasyApi
    {
        /// <summary>
        /// Name of the API
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id of the API
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Binding associated with the API
        /// </summary>
        public string Binding { get; set; }
    }
}
