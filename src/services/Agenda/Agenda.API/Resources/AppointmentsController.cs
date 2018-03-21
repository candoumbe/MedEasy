using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agenda.API.Routing;
using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DTO;
using MedEasy.Core.Attributes;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Optional;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Agenda.API.Controllers
{
    [Route("agenda/[controller]")]
    public class AppointmentsController
    {
        private readonly IUrlHelper _urlHelper;
        private readonly IMediator _mediator;
        private readonly IOptionsSnapshot<AgendaApiOptions> _apiOptions;

        /// <summary>
        /// Name of the endpoint
        /// </summary>²
        public static string EndpointName => nameof(AppointmentsController).Replace("Controller", string.Empty);

        /// <summary>
        /// Builds a new <see cref="AppointmentsController"/>
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="mediator"></param>
        /// <param name="apiOptions"></param>
        public AppointmentsController(IUrlHelper urlHelper, IMediator mediator, IOptionsSnapshot<AgendaApiOptions> apiOptions)
        {
            _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiOptions = apiOptions ?? throw new ArgumentNullException(nameof(apiOptions));
        }



        /// <summary>
        /// Gets the appointments
        /// </summary>
        /// <param name="pagination"></param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns></returns>
        /// <response code="200"/>

        [HttpGet]
        [ProducesResponseType(typeof(GenericPagedGetResponse<BrowsableResource<AppointmentInfo>>), Status200OK)]
        public async Task<IActionResult> Get([FromQuery, Minimum(1)] int page, [FromQuery, Minimum(1)] int pageSize, CancellationToken ct = default)
        {

            PaginationConfiguration pagination = new PaginationConfiguration { Page = page, PageSize = pageSize };
            pagination.PageSize = Math.Min(_apiOptions.Value.MaxPageSize, pagination.PageSize);

            
            Page<AppointmentInfo> result = await _mediator.Send(new GetPageOfAppointmentInfoQuery(pagination), ct)
                .ConfigureAwait(false);


            IEnumerable<BrowsableResource<AppointmentInfo>> entries = result.Entries
                .Select(x => new BrowsableResource<AppointmentInfo>
                {
                    Resource = x,
                    Links = new[]
                    {
                        new Link { Relation = LinkRelation.Self, Method = "GET", Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, x.Id})}
                    }
                });


            GenericPagedGetResponse<BrowsableResource<AppointmentInfo>> response = new GenericPagedGetResponse<BrowsableResource<AppointmentInfo>>(
                entries,
                first: _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, Page = 1, pagination.PageSize }),
                previous : pagination.Page > 1 && result.Count > 1
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, Page = pagination.Page - 1, pagination.PageSize })
                    : null,

                next: result.Count > pagination.Page
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, Page = pagination.Page + 1, pagination.PageSize })
                    : null,
                last: _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, Page = Math.Max(1, result.Count), pagination.PageSize}),
                count : result.Total
            );

            IActionResult actionResult = new OkObjectResult(response);

            return actionResult;
        }

        /// <summary>
        /// Gets a appointment by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">id of the appointment to get</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <response code="200">The appointment with the specified id</response>
        /// <response code="404">resource not found</response>
        /// <response code="400"><paramref name="id"/> was not sent or was empty</response>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BrowsableResource<AppointmentInfo>), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(typeof(ErrorObject), Status400BadRequest)]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
        {
            Option<AppointmentInfo> optionalAppointment = await _mediator.Send(new GetOneAppointmentInfoByIdQuery(id), ct)
                .ConfigureAwait(false);

            return optionalAppointment.Match<IActionResult>(
                some: appointment => {

                    BrowsableResource<AppointmentInfo> result = new BrowsableResource<AppointmentInfo>
                    {
                        Resource = appointment,
                        Links = new[]
                        {
                            new Link { Relation = LinkRelation.Self, Method = "GET",  Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, appointment.Id})},
                            new Link { Relation = "delete", Method = "DELETE", Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, appointment.Id})}
                        }
                    };

                    return new OkObjectResult(result);
                },
                none: () => new NotFoundResult()
            );
        }
    }
}
