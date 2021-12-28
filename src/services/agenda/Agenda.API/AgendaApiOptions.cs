namespace Agenda.API
{
    /// <summary>
    /// Gives strongly typed access to API's options.
    /// </summary>
    public class AgendaApiOptions
    {
        /// <summary>
        /// Number of items to return when requiring a page of result no hint was provided by the client.
        /// </summary>
        public int DefaultPageSize { get; set; }

        /// <summary>
        /// Number of items the API can return in a single call at most
        /// </summary>
        public int MaxPageSize { get; set; }
    }
}
