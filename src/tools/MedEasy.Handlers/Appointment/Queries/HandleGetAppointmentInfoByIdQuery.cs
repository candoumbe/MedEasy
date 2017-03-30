using MedEasy.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using MedEasy.DTO;
using MedEasy.Validators;
using AutoMapper.QueryableExtensions;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using MedEasy.Handlers.Core.Appointment.Queries;

namespace MedEasy.Handlers.Appointment.Queries
{
    /// <summary>
    /// An instance of this class can be used to handle <see cref="IWantOneAppointmentInfoByIdQuery"/> interface implementations
    /// </summary
    public class HandleGetAppointmentInfoByIdQuery : GenericGetOneByIdQueryHandler<Guid, Objects.Appointment, Guid, AppointmentInfo, IWantOneResource<Guid, Guid, AppointmentInfo>, IValidate<IWantOneResource<Guid, Guid, AppointmentInfo>>>, IHandleGetAppointmentInfoByIdQuery
    {

        /// <summary>
        /// Builds a new <see cref="HandleGetAppointmentInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="factory">factory to use to retrieve <see cref="Objects.Appointment"/> instances</param>
        /// <param name="logger">a logger</param>
        /// <param name="expressionBuilder"></param>
        public HandleGetAppointmentInfoByIdQuery(IUnitOfWorkFactory factory, ILogger<HandleGetAppointmentInfoByIdQuery> logger, IExpressionBuilder expressionBuilder) : base(Validator<IWantOneResource<Guid, Guid, AppointmentInfo>>.Default, logger, factory, expressionBuilder)
        {
        }
    }
}