using MedEasy.Commands;
using MedEasy.Handlers;
using MedEasy.Handlers.Commands;
using System;
using System.Threading.Tasks;

namespace MedEasy.Pipeline
{
    /// <summary>
    /// Define the contract for a post processor Handler
    /// </summary>
    /// <typeparam name="TKey">Type of the <typeparamref name="TCommand"/> identifier.</typeparam>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TCommand">Type of the command</typeparam>
    public interface IPostCommandHandler<TKey, TInput, TOutput> 
    {
        /// <summary>
        /// Handle the 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        Task HandleAsync(TInput input, TOutput output);
    }

    
}
