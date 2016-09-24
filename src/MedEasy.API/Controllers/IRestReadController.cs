using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MedEasy.DTO;
using System;
using MedEasy.Objects;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Defines the contract for controllers that are RESTFULL
    /// </summary>
    /// <typeparam name="TKey">Type of the key</typeparam>
    /// <typeparam name="TEntityInfo">Type of the entity</typeparam>
    public interface IRestController<TKey, TEntityInfo>
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