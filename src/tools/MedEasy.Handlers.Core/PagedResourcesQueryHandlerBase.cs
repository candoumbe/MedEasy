﻿using MedEasy.Validators;
using System;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using MedEasy.Queries;
using System.Threading;
using System.Collections.Generic;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System.Linq;
using MedEasy.Handlers.Core.Exceptions;
using System.Linq.Expressions;
using static MedEasy.Validators.ErrorLevel;
using AutoMapper.QueryableExtensions;

namespace MedEasy.Handlers.Core.Queries
{
    /// <summary>
    /// Base class that can be used to quickly create a command handler.
    /// </summary>
    /// <remarks>
    ///     Queries processed by <see cref="PagedResourcesQueryHandlerBase{TKey, TEntity, TData, TOutput, TQuery, TQueryValidator}"/> outputs
    /// </remarks>
    /// <typeparam name="TKey">Type of the identifiar of the command</typeparam>
    /// <typeparam name="TResult">Type of the expected <see cref="IPagedResult{T}"/> returned by <see cref="IHandleQueryAsync{TKey, TData, TResult, TQuery}.HandleAsync(TQuery, CancellationToken)"/></typeparam>
    /// <typeparam name="TQuery">Type of the query</typeparam>
    /// <typeparam name="TEntity">Type of the entity store will be handled against</typeparam>
    public abstract class PagedResourcesQueryHandlerBase<TKey, TEntity, TResult, TQuery> : QueryHandlerBase<TKey, TEntity, PaginationConfiguration, IPagedResult<TResult>, TQuery>
        where TQuery : IWantPageOfResources<TKey, PaginationConfiguration, TResult>
        where TKey : IEquatable<TKey>
        where TEntity : class
    {

        protected IExpressionBuilder ExpressionBuilder { get; }

        /// <summary>
        /// Builds a new <see cref="PagedResourcesQueryHandlerBase{TKey, TEntity, TData, TOutput, TQuery, TQueryValidator}"/>
        /// </summary>
        /// <param name="uowFactory">Factory used for accessing <see cref="TEntity"/> instances</param>
        /// <param name="expressionBuilder">Builds expression to map <see cref="TEntity"/> to <see cref="TResult"/></param>
        /// <exception cref="ArgumentNullException">if <paramref name="expressionBuilder"/> is <c>null</c></exception>
        protected PagedResourcesQueryHandlerBase(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
            : base(uowFactory)
        {
            ExpressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

    
        public override async ValueTask<IPagedResult<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            using (IUnitOfWork uow = UowFactory.New())
            {
                PaginationConfiguration data = query.Data;

                Expression<Func<TEntity, TResult>> selector = ExpressionBuilder.GetMapExpression<TEntity, TResult>();
                IPagedResult<TResult> output = await uow.Repository<TEntity>()
                    .ReadPageAsync(selector, data.PageSize, data.Page, cancellationToken: cancellationToken);

                //Logger.LogInformation($"Query {query.Id} processed successfully");

                return output;
            }
        }
    }
}