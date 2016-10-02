using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Queries;
using MedEasy.Queries;

namespace MedEasy.Handlers.Specialty.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOneSpecialtyInfoByIdQuery"/> interface implementations
    /// </summary
    public class HandleGetManySpecialtyInfoQuery : GenericGetManyQueryHandler<Guid, Objects.Specialty, int, SpecialtyInfo, IWantManyResources<Guid, SpecialtyInfo>>, IHandleGetManySpecialtyInfosQuery
    {

        /// <summary>
        /// Builds a new <see cref="HandleGetSpecialtyInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Specialty"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder">Builder for <see cref="System.Linq.Expressions.Expression{TDelegate}"/>that can map <see cref="Objects.Specialty"/> instances to <see cref="SpecialtyInfo"/> instances</param>
        public HandleGetManySpecialtyInfoQuery(IUnitOfWorkFactory factory, ILogger<HandleGetManySpecialtyInfoQuery> logger, IExpressionBuilder expressionBuilder) : base(logger, factory, expressionBuilder)
        {
        }
    }
}