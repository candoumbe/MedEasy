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
    /// Query to retrieve a planning for the specified participant between two date.
    /// </summary>
    public class GetPlanningByParticipantIdQuery : QueryBase<Guid, (Guid participantId, DateTimeOffset start, DateTimeOffset end), Option<IEnumerable<AppointmentInfo>>>
    {
        /// <summary>
        /// Builds a new <see cref="GetPlanningByParticipantIdQuery"/> instance
        /// </summary>
        /// <param name="participantId">Id of the participant.</param>
        /// <param name="start">Start of the interval</param>
        /// <param name="end">End of the interval (inclusive).</param>
        /// <param name="page">index of the page of result</param>
        /// <param name="pageSize">number of items per page.</param
        public GetPlanningByParticipantIdQuery(Guid participantId, DateTimeOffset from, DateTimeOffset to)
            : base(Guid.NewGuid(), (participantId, from, to))
        {
            if (participantId == default)
            {
                throw new ArgumentOutOfRangeException(nameof(participantId), participantId, "participantId cannot be empty");
            }

            if (from > to)
            {
                throw new ArgumentException("The specified interval is not a valid period");
            }
        }
    }
}
