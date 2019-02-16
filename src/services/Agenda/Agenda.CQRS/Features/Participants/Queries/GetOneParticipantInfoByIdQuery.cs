using Agenda.DTO;
using MedEasy.CQRS.Core.Queries;
using Optional;
using System;

namespace Agenda.CQRS.Features.Participants.Queries
{
    /// <summary>
    /// A query to get an <see cref="ParticipantInfo"/>
    /// </summary>
    public class GetOneParticipantInfoByIdQuery : GetOneResourceQuery<Guid, Guid, Option<ParticipantInfo>>, IEquatable<GetOneParticipantInfoByIdQuery>
    {
        /// <summary>
        /// Builds a new <see cref="GetOneParticipantInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="participantId">id of the <see cref="ParticipantInfo"/> to retrieve</param>
        /// <exception cref="ArgumentException">if <paramref name="participantId"/> is <see cref="Guid.Empty"/></exception>
        public GetOneParticipantInfoByIdQuery(Guid participantId) : base(Guid.NewGuid(), participantId)
        {
            if (participantId == default)
            {
                throw new ArgumentException($"{nameof(participantId)} cannot be empty", nameof(participantId));
            }
        }

        public override bool Equals(object other) => Equals(other as GetOneParticipantInfoByIdQuery);

        public bool Equals(GetOneParticipantInfoByIdQuery other) => other != null
                && (ReferenceEquals(this, other) || Equals(Data, other.Data));

        public override int GetHashCode() => base.GetHashCode();
    }
}
