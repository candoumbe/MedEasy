namespace MedEasy.CQRS.Core.Exceptions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Exception thrown when a query is not valid
    /// </summary>
    /// <typeparam name="TQueryId">Type of the  query ID that causes the exception to be thrown</typeparam>
    public class QueryNotValidException<TQueryId> : ValidationException
    {
        /// <summary>
        /// Id of the query that causes the exception to be thrown
        /// </summary>
        public TQueryId QueryId { get; }

        /// <summary>
        /// Builds a new <see cref="QueryNotValidException{TQueryId}"/> instance
        /// </summary>
        /// <param name="queryId"><see cref="IQuery.Id"/> that cause the exception to be thrown</param>
        /// <param name="errors">errors that causes the exception to be thrown</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="queryId"/> is equals to default value of <see cref="TQueryId"/></exception>
        public QueryNotValidException(TQueryId queryId, IEnumerable<ErrorInfo> errors) : base(errors)
        {
            if (Equals(default(TQueryId), queryId))
            {
                throw new ArgumentOutOfRangeException(nameof(queryId), $"{nameof(queryId)} must not be set to default value");
            }
            QueryId = queryId;
        }
    }
}
