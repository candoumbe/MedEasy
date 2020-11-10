using Agenda.API.Resources.v1.Appointments;
using Agenda.API.Routing;
using Agenda.CQRS.Features.Appointments.Commands;
using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using Agenda.Models.v1.Appointments;
using AutoMapper;
using MedEasy.Attributes;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Forms;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Forms.LinkRelation;
using Microsoft.AspNetCore.Routing;
using MedEasy.Models;

namespace Agenda.API.Resources.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    public class AppointmentsController
    {
        private readonly LinkGenerator _urlHelper;
        private readonly IMediator _mediator;
        private readonly IOptionsSnapshot<AgendaApiOptions> _apiOptions;
        private readonly IMapper _mapper;
        private readonly ApiVersion _apiVersion;

        /// <summary>
        /// Name of the endpoint
        /// </summary>²
        public static string EndpointName => nameof(AppointmentsController).Replace("Controller", string.Empty);

        /// <summary>
        /// Builds a new <see cref="AppointmentsController"/>
        /// </summary>
        /// <param name="urlHelper">Helper to build urls</param>
        /// <param name="mediator"></param>
        /// <param name="apiOptions"></param>
        /// <param name="mapper"></param>
        /// <param name="apiVersion"></param>
        public AppointmentsController(LinkGenerator urlHelper, IMediator mediator, IOptionsSnapshot<AgendaApiOptions> apiOptions, IMapper mapper, ApiVersion apiVersion)
        {
            _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _apiOptions = apiOptions ?? throw new ArgumentNullException(nameof(apiOptions));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _apiVersion = apiVersion;
        }

        /// <summary>
        /// Creates a new appointment resource
        /// </summary>
        /// <param name="newAppointment">data of the appointment</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <response code="400"></response>
        [HttpPost]
        [ProducesResponseType(typeof(Browsable<AppointmentModel>), Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
        [ProducesResponseType(Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] NewAppointmentModel newAppointment, CancellationToken ct = default)
        {
            NewAppointmentInfo info = _mapper.Map<NewAppointmentInfo>(newAppointment);
            AppointmentInfo newResource = await _mediator.Send(new CreateAppointmentInfoCommand(info), ct)
                                                         .ConfigureAwait(false);
            string version = _apiVersion.ToString();
            IEnumerable<AttendeeInfo> participants = newResource.Attendees;
            IEnumerable<Link> links = new List<Link>(1 + participants.Count())
            {
                new Link
                {
                    Relation = Self,
                    Method = "GET",
                    Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi,
                                                    new { controller = EndpointName, newResource.Id, version })
                }
            }
            .Concat(participants.Select(p => new Link
            {
                Relation = $"get-participant-{p.Id}",
                Method = "GET",
                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = AttendeesController.EndpointName, p.Id, version })
            })
            )
#if DEBUG
            .ToArray()
#endif
            ;

            Browsable<AppointmentInfo> browsableResource = new Browsable<AppointmentInfo>
            {
                Resource = newResource,
                Links = links
            };

            return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, newResource.Id, version }, browsableResource);
        }

        /// <summary>
        /// Gets the appointments
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct">Notifies to cancel the execution of the request</param>
        /// <returns></returns>
        /// <response code="200"/>
        /// <response code="400"><paramref name="page"/> or <paramref name="pageSize"/> is negative or 0</response>
        [HttpGet]
        [HttpHead]
        [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
        public async Task<ActionResult<GenericPageModel<Browsable<AppointmentModel>>>> Get([Minimum(1)] int page, [Minimum(1)] int pageSize, CancellationToken ct = default)
        {
            PaginationConfiguration pagination = new PaginationConfiguration
            {
                Page = page,
                PageSize = Math.Min(_apiOptions.Value.MaxPageSize, pageSize)
            };

            Page<AppointmentInfo> result = await _mediator.Send(new GetPageOfAppointmentInfoQuery(pagination), ct)
                .ConfigureAwait(false);

            string version = _apiVersion.ToString();
            IEnumerable<Browsable<AppointmentModel>> entries = _mapper.Map<IEnumerable<AppointmentModel>>(result.Entries)
                .Select(x => new Browsable<AppointmentModel>
                {
                    Resource = x,
                    Links = new[]
                    {
                        new Link {
                            Relation = Self,
                            Method = "GET",
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi,
                                                            new {controller = EndpointName, x.Id, version})}
                    }
                });

            return new GenericPageModel<Browsable<AppointmentModel>>
            {
                Items = entries,
                Links = new PageLinksModel
                {
                    First = new Link
                    {
                        Method = "GET",
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, Page = 1, pagination.PageSize, version }),
                        Relation = First
                    },
                    Previous = pagination.Page > 1 && result.Count > 1
                            ? new Link
                            {
                                Method = "GET",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, Page = 1, pagination.PageSize, version }),
                                Relation = Previous
                            }
                            : null,
                    Next = result.Count > pagination.Page
                            ? new Link
                            {
                                Method = "GET",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, Page = pagination.Page + 1, pagination.PageSize, version }),
                                Relation = Next
                            }
                            : null,
                    Last = new Link
                    {
                        Method = "GET",
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, Page = Math.Max(1, result.Count), pagination.PageSize, version }),
                        Relation = Last
                    },
                },
                Total = result.Total
            };
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
        [HttpHead("{id}")]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
        public async Task<ActionResult<Browsable<AppointmentModel>>> Get(Guid id, CancellationToken ct = default)
        {
            Option<AppointmentInfo> optionalAppointment = await _mediator.Send(new GetOneAppointmentInfoByIdQuery(id), ct)
                .ConfigureAwait(false);

            return optionalAppointment.Match<ActionResult<Browsable<AppointmentModel>>>(
                some: appointment =>
                {
                    string version = _apiVersion.ToString();
                    IList<Link> links = new List<Link>
                    {
                        new Link
                        {
                            Relation = Self,
                            Method = "GET",
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, appointment.Id, version})
                        },
                        new Link
                        {
                            Relation = "delete",
                            Method = "DELETE",
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {controller = EndpointName, appointment.Id, version})
                        }
                    };

                    return new Browsable<AppointmentModel>
                    {
                        Resource = _mapper.Map<AppointmentModel>(appointment),
                        Links = links
                    };
                },
                none: () => new NotFoundResult()
            );
        }

        /// <summary>
        /// Search `appointments` resources based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <param name="ct">Notfies to cancel the search operation</param>
        /// <remarks>
        /// <para>All criteria are combined as a AND.</para>
        /// <para>
        /// Advanded search :
        /// Several operators that can be used to make an advanced search :
        /// '*' : match zero or more characters in a string property.
        /// </para>
        /// <para>
        ///     // GET api/appointments/search?location=Gotham
        ///     will match all resources which have exactly 'Gotham' in the `location` property
        /// </para>
        /// <para>
        ///     // GET api/appointments/search?location=C*tral
        ///     will match match all resources which starts with 'C' and ends with 'tral'.
        /// </para>
        /// <para>'?' : match exactly one charcter in a string property.</para>
        /// <para>'!' : negate a criterion</para>
        /// <para>
        ///     // GET api/appointments/search?location=!Gotham
        ///     will match all resources where "location" is not "Gotham"
        /// </para>
        ///     
        /// </remarks>
        /// <response code="200">"page" of resources that matches <paramref name="search"/> criteria.</response>
        /// <response code="400">one of the search criterion is not valid</response>
        [HttpGet("[action]")]
        [HttpHead("[action]")]
        [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
        public async Task<ActionResult<GenericPageModel<Browsable<AppointmentModel>>>> Search([FromQuery] SearchAppointmentModel search, CancellationToken ct = default)
        {
            search.PageSize = Math.Min(search.PageSize, _apiOptions.Value.MaxPageSize);

            SearchAppointmentInfo data = _mapper.Map<SearchAppointmentInfo>(search);

            Page<AppointmentInfo> page = await _mediator.Send(new SearchAppointmentInfoQuery(data), ct)
                .ConfigureAwait(false);

            string version = _apiVersion.ToString();

            return new GenericPageModel<Browsable<AppointmentModel>>
            {
                Items = _mapper.Map<IEnumerable<AppointmentModel>>(page.Entries).Select(x => new Browsable<AppointmentModel>
                {
                    Resource = x,
                    Links = new[]
                    {
                        new Link { Relation = Self, Method = "GET", Href =_urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, x.Id, version })}
                    }
                }),
                Links = new PageLinksModel
                {
                    First = new Link
                    {
                        Relation = First,
                        Method = "GET",
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.From, search.To, search.Sort, page = 1, search.PageSize, version })
                    },
                    Previous = page.Count > 1 && search.Page > 1
                        ? new Link
                        {
                            Method = "GET",
                            Relation = Previous,
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.From, search.To, search.Sort, page = search.Page - 1, search.PageSize, version })
                        }
                        : null,
                    Next = search.Page < page.Count
                    ? new Link
                    {
                        Relation = Next,
                        Method = "GET",
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.From, search.To, search.Sort, page = search.Page + 1, search.PageSize, version })
                    }
                    : null,
                    Last = new Link
                    {
                        Relation = Last,
                        Method = "GET",
                        Href = _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.From, search.To, search.Sort, page = page.Count, search.PageSize, version })
                    }
                },
                Total = page.Total
            };
        }

        /// <summary>
        /// Removes the attendee with the specified <paramref name="attendeeId"/> from the appointment with 
        /// the specified <paramref name="id"/>
        /// </summary>
        /// <param name="id">id of the appointment where to remove the participant from</param>
        /// <param name="attendeeId">id of the attendee to remove</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <response code="204">Attendee was successfully removed</response>
        /// <response code="404">Attendee or apppointment not found</response>
        /// <response code="409">Attendee cannot be removed</response>
        /// <response code="400"></response>
        [HttpDelete("{id}/attendees/{attendeeId}")]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status409Conflict)]
        [ProducesResponseType(typeof(ValidationProblemDetails), Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id, Guid attendeeId, CancellationToken ct = default)
        {
            RemoveAttendeeFromAppointmentByIdCommand cmd = new RemoveAttendeeFromAppointmentByIdCommand(data: (id, attendeeId));

            DeleteCommandResult cmdResult = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);

            return cmdResult switch
            {
                DeleteCommandResult.Done => new NoContentResult(),
                DeleteCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                DeleteCommandResult.Failed_NotFound => new NotFoundResult(),
                DeleteCommandResult.Failed_Conflict => new StatusCodeResult(Status409Conflict),
                _ => throw new ArgumentOutOfRangeException(nameof(cmdResult), cmdResult, "Unexpected delete result."),
            };
        }
    }
}
