namespace Agenda.CQRS.Features.Appointments.Queries
{
    using Agenda.DTO;
    using Agenda.DTO.Resources.Search;

    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Repositories;

    using System;

    /// <summary>
    /// Query to search <see cref="AppointmentInfo"/>s
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    public class SearchAppointmentInfoQuery : QueryBase<Guid, SearchAppointmentInfo, Page<AppointmentInfo>>
    {
        /// <summary>
        /// Builds a new <see cref="SearchAppointmentInfoQuery"/> instance.
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="ArgumentException">if <paramref name="data"/> is <c>null</c> </exception>
        public SearchAppointmentInfoQuery(SearchAppointmentInfo data) : base(Guid.NewGuid(), data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
    }
}