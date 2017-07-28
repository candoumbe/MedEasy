using System.Threading.Tasks;

namespace MedEasy.Handlers.Core
{
    /// <summary>
    /// Runs <strong>after</strong> the command
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public interface IPostProcess<in TRequest, in TResponse>
    {
        /// <summary>
        /// Asynchronously process the data 
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        Task PostProcess(TRequest request, TResponse response);
    }
}