using System.Threading.Tasks;

namespace MedEasy.Handlers.Core
{
    /// <summary>
    /// Performs an action based on the input
    /// </summary>
    /// <typeparam name="T">Type of the input to process</typeparam>
    public interface IPreprocess<T>
    {
        /// <summary>
        /// Performs an action
        /// </summary>
        /// <param name="input">current input</param>
        /// <returns></returns>
        Task Preprocess(T input);
    }
}