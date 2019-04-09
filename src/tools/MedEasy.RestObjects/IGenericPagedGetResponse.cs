namespace MedEasy.RestObjects
{
    public interface IGenericPagedGetResponse
    {
        /// <summary>
        /// Total of items in the the result
        /// </summary>
        long Total { get; }

        /// <summary>
        /// Number of items in the the result
        /// </summary>
        long Count { get; }
    }
}