using System;

namespace MedEasy.API.Models
{
    public interface IResourceModel<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Id of the resource
        /// </summary>
        TKey Id { get; set; }
    }
}