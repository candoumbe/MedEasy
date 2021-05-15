namespace Agenda.CQRS.Features.Participants.Queries
{
    using Agenda.DTO;
    using Agenda.Ids;

    using MedEasy.CQRS.Core.Queries;

    using Optional;

    using System;

    /// <summary>
    /// A query to get an <see cref="AttendeeInfo"/>
    /// </summary>
    public class GetOneAttendeeInfoByIdQuery : GetOneResourceQuery<Guid, AttendeeId, Option<AttendeeInfo>>, IEquatable<GetOneAttendeeInfoByIdQuery>
    {
        /// <summary>
        /// Builds a new <see cref="GetOneAttendeeInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="attendeeId">id of the <see cref="AttendeeInfo"/> to retrieve</param>
        /// <exception cref="ArgumentException">if <paramref name="attendeeId"/> is <see cref="Guid.Empty"/></exception>
        public GetOneAttendeeInfoByIdQuery(AttendeeId attendeeId) : base(Guid.NewGuid(), attendeeId)
        {
            if (attendeeId == default)
            {
                throw new ArgumentException($"{nameof(attendeeId)} cannot be empty", nameof(attendeeId));
            }
        }

        public override bool Equals(object obj) => Equals(obj as GetOneAttendeeInfoByIdQuery);

        public bool Equals(GetOneAttendeeInfoByIdQuery other) => other != null
                && (ReferenceEquals(this, other) || Equals(Data, other.Data));

        public override int GetHashCode() => base.GetHashCode();
    }
}
