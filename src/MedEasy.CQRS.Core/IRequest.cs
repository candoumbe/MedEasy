using System;

namespace MedEasy.CQRS.Core
{
    /// <summary>
    /// Marker interface for request that should return data.
    /// </summary>
    /// <typeparam name="TResponse">Type of the expected response.</typeparam>
    /// <typeparam name="TRequestId">Type of the request identitier.</typeparam>
    public interface IRequest<TRequestId, out TResponse>
        where TRequestId : IEquatable<TRequestId>
    {
        /// <summary>
        /// Request identifier
        /// </summary>
        TRequestId Id { get; }
    }


    /// <summary>
    /// Marker interface for request that return no data.
    /// </summary>
   public interface IRequest : IRequest<Guid, Nothing> { }



}
