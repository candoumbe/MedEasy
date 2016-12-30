
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


    /// <summary>
    /// Defines the basic shape of a typed filter
    /// </summary>
    /// <typeparam name="T">Type the filter will be applied on</typeparam>
    public interface IDataFilter<T> : IDataFilter
    {

    }
}
