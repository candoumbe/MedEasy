using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Defines the contract for controllers that are RESTFULL
    /// </summary>
    /// <typeparam name="TKey">Type of the key</typeparam>
    public interface IRestReadController<TKey>
        where TKey : IEquatable<TKey>
    {
        
        /// <summary>
        /// Asynchronously gets the resource with the specified <paramref name="id"/> 
        /// </summary>
        /// <param name="id">id of the resource</param>
        /// <returns></returns>
        Task<IActionResult> Get(TKey id);
    }
}