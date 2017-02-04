using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using AutoMapper.QueryableExtensions;
using MedEasy.Queries;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Handlers.Core.Appointment.Queries;

namespace MedEasy.Handlers.Appointment.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOneAppointmentInfoByIdQuery"/> interface implementations
    /// </summary
    public class HandleGetManyAppointmentInfoQuery : GenericGetManyQueryHandler<Guid, Objects.Appointment, int, AppointmentInfo, IWantManyResources<Guid, AppointmentInfo>>, IHandleGetManyAppointmentInfosQuery
    {

        /// <summary>
        /// Builds a new <see cref="HandleGetAppointmentInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Appointment"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder">Builder for <see cref="System.Linq.Expressions.Expression{TDelegate}"/>that can map <see cref="Objects.Appointment"/> instances to <see cref="AppointmentInfo"/> instances</param>
        public HandleGetManyAppointmentInfoQuery(IUnitOfWorkFactory factory, ILogger<HandleGetManyAppointmentInfoQuery> logger, IExpressionBuilder expressionBuilder) : base(logger, factory, expressionBuilder)
        {
        }
    }
}