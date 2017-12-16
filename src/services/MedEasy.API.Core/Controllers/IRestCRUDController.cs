using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MedEasy.API.Core.Controllers
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



        ///// <summary>
        ///// Asynchronously gets all the resource
        ///// </summary>
        ///// <param name="query"></param>
        ///// <returns><see cref="BrowsableResource{TEntityInfo}"/></returns>
        //Task<IActionResult> List(PaginationConfiguration query);

        ///// <summary>
        ///// Asynchronously updates the specified resource
        ///// </summary>
        ///// <param name="key">id of the resource</param>
        ///// <param name="info">resource with values to update</param>
        ///// <returns><see cref="BrowsableResource{TEntityInfo}"/></returns>
        //Task<BrowsableResource<TEntityInfo>> Put(TKey key, TPut info);

        ///// <summary>
        ///// Asynchronously create a <typeparam ref="TCreate"/> resource
        ///// </summary>
        ///// <param name="cmd">command to create a resource</param>
        ///// <returns><see cref="BrowsableResource{TEntityInfo}"/></returns>
        //Task<BrowsableResource<TEntityInfo>> Post(TCreateCommand cmd);
    }
}