using System;

namespace MedEasy.API.Models
{
    /// <summary>
    /// Definies the shape of a resource
    /// </summary>
    /// <typeparam name="TKey">Type of the resource identifier</typeparam>
    public interface IResourceModel<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Id of the resource
        /// </summary>
        TKey Id { get; set; }
    }
}