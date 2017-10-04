using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MedEasy.Measures.API.Controllers
{


    /// <summary>
    /// Defines the contract for controllers that are RESTFULL and CRUD
    /// </summary>
    /// <typeparam name="TKey">Type of the key</typeparam>
    /// <typeparam name="TPost">Type of the input parameter the command to create a resource</typeparam>
    public interface IRestCRUDController<TKey, TPost> : IRestReadController<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Asynchronously deletes the resource with the specified <paramref name="id"/>
        /// </summary>
        /// <param name="id">id of the ressource to delete</param>
        /// <returns><see cref="OkResult"/> if the operation succeed</returns>
        Task<IActionResult> Delete(TKey id);

        /// <summary>
        /// Asynchronously creates a new resource
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        Task<IActionResult> Post(TPost info);
    }
}