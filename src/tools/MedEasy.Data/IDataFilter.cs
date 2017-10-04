
namespace MedEasy.Data
{
    /// <summary>
    /// Defines the basic shape of a filter
    /// </summary>
    public interface IDataFilter
    {
        /// <summary>
        /// Gets the JSON representation of the filter
        /// </summary>
        /// <returns></returns>
        string ToJson();
    }
}
