using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.CQRS.Core.Queries
{
    public abstract class QueryBase<TKey, TData, TResult> : IQuery<TKey, TData, TResult>
        where TKey : IEquatable<TKey>
    {

        /// <summary>
        /// Query's identifier. 
        /// 
        /// </summary>
        public TKey Id { get; }

        /// <summary>
        /// Data the query carries
        /// </summary>
        public TData Data { get; }

        /// <summary>
        /// Create 
        /// </summary>
        /// <param name="queryId">Query's identifier. Should be unique</param>
        /// <param name="data">Data the query "carries"</param>
        protected QueryBase(TKey queryId, TData data)
        {
            Id = queryId;
            Data = data;
        }
    }
}
