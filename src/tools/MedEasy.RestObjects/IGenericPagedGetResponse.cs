namespace MedEasy.RestObjects
{
    public interface IGenericPagedGetResponse
    {
        /// <summary>
        /// Total of items in the the result
        /// </summary>
        int Total { get; }

        /// <summary>
        /// Number of items in the the result
        /// </summary>
        int Count { get; }
    }
}