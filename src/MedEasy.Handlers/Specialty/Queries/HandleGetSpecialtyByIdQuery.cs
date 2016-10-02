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
    public class HandleGetSpecialtyInfoByIdQuery : GenericGetOneByIdQueryHandler<Guid, Objects.Specialty, int, SpecialtyInfo, IWantOneResource<Guid, int, SpecialtyInfo>>, IHandleGetSpecialtyInfoByIdQuery
    {

        /// <summary>
        /// Builds a new <see cref="HandleGetSpecialtyInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Specialty"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder"></param>
        public HandleGetSpecialtyInfoByIdQuery(IUnitOfWorkFactory factory, ILogger<HandleGetSpecialtyInfoByIdQuery> logger, IExpressionBuilder expressionBuilder) : base(logger, factory, expressionBuilder)
        {
        }
    }
}