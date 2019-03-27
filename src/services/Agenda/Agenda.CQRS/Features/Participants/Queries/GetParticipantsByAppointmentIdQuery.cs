using Agenda.DTO;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Participants.Queries
{
    /// <summary>
    /// Query to list all <see cref="AppointmentInfo"/> for participant
    /// </summary>
    public class GetParticipantsByAppointmentIdQuery : QueryBase<Guid, (Guid participantId, DateTimeOffset start, DateTimeOffset end, int page, int pageSize), Option<Page<AppointmentInfo>>>
    {
        /// <summary>
        /// Creates a new <see cref="GetParticipantsByAppointmentIdQuery"/> instance.
        /// </summary>
        /// <param name="participantId">Id of the participant.</param>
        /// <param name="start">Start of the interval</param>
        /// <param name="end">End of the interval (inclusive).</param>
        /// <param name="page">index of the page of result</param>
        /// <param name="pageSize">number of items per page.</param
        public GetParticipantsByAppointmentIdQuery(Guid participantId, DateTimeOffset start, DateTimeOffset end, int page, int pageSize)
            : base(Guid.NewGuid(), (participantId, start, end, page, pageSize))
        {

        }
    }
}
