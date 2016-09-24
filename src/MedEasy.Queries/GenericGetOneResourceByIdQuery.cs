using System;

namespace MedEasy.Queries
{
    /// <summary>
    /// Base class for creating queries that request a resource by its id;
    /// </summary>
    /// <typeparam name="TKey">Type of the resource identifier</typeparam>
    /// <typeparam name="TResult">Type of the resource</typeparam>
    public class GenericGetOneResourceByIdQuery<TKey, TResult> : IWantOneResource<Guid, TKey, TResult>
    {
        public Guid Id { get; } 

        public TKey Data { get; set; }


        public GenericGetOneResourceByIdQuery(TKey objKey)
        {
            Id = Guid.NewGuid();
            Data = objKey;
        }


    }
}
