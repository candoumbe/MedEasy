using Agenda.API.Controllers;
using Agenda.API.Routing;
using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using DataFilters;
using MedEasy.Core.Attributes;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.DTO.Search;
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
using static MedEasy.RestObjects.LinkRelation;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace Agenda.API.Resources
{
    [Route("agenda/[controller]")]
    [ApiController]
    public class ParticipantsController
    {
        private readonly IUrlHelper _urlHelper;
        private readonly IMediator _mediator;
        private readonly IOptionsSnapshot<AgendaApiOptions> _apiOptions;

        /// <summary>
        /// Name of the endpoint
        /// </summary>²
        public static string EndpointName => nameof(ParticipantsController).Replace("Controller", string.Empty);

        public ParticipantsController(IUrlHelper urlHelper, IMediator mediator, IOptionsSnapshot<AgendaApiOptions> apiOptions)
        {
            _urlHelper = urlHelper;
            _mediator = mediator;
            _apiOptions = apiOptions;
        }

        /// <summary>
        /// Gets a page of participants resources. 
        /// The number of resources the page contains may be less than <see cref="pageSize"/>
        /// </summary>
        /// <param name="page">index of the page of results (1 for the first page, 2 for the next, ...)</param>
        /// <param name="pageSize">Number of resources per page</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpHead]
        [HttpGet]
        public async Task<IActionResult> Get([Minimum(1)] int page, [Minimum(1)] int pageSize, CancellationToken ct = default)
        {
            pageSize = Math.Min(pageSize, _apiOptions.Value.MaxPageSize);
            Page<ParticipantInfo> result = await _mediator.Send(new GetPageOfParticipantInfoQuery(page, pageSize), ct)
                .ConfigureAwait(false);

            GenericPagedGetResponse<Browsable<ParticipantInfo>> resources = new GenericPagedGetResponse<Browsable<ParticipantInfo>>(

                result.Entries.Select(x => new Browsable<ParticipantInfo>
                {
                    Resource = x,
                    Links = new[]
                    {
                        new Link
                        {
                            Relation = Self,
                            Method = "GET",
                            Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, x.Id })
                        }
                    }
                }),
                first: _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = 1, pageSize }),
                previous: result.Count > 2 && page > 1
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = page - 1, pageSize })
                    : null,
                next: page < result.Count
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = page + 1, pageSize })
                    : null,

                last: _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = result.Count, pageSize }),
                total: result.Total
            );

            return new OkObjectResult(resources);
        }

        /// <summary>
        /// Gets a participant resouurce by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">id of the resource to look for</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <response code="404"></response>

        [HttpGet("{id}")]
        [HttpHead("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
        {
            Option<ParticipantInfo> optionalResource = await _mediator.Send(new GetOneParticipantInfoByIdQuery(id), ct)
                .ConfigureAwait(false);

            return optionalResource.Match<IActionResult>(
                some: resource =>
                {
                    Browsable<ParticipantInfo> browsableResource = new Browsable<ParticipantInfo>
                    {
                        Resource = resource,
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = Self,
                                Method = "GET",
                                Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id })
                            },
                        }
                    };

                    return new OkObjectResult(resource);
                },

                none: () => new NotFoundResult());
        }

        /// <summary>
        /// Gets a list of appointments for the participant with the specified id
        /// </summary>
        /// <param name="id">id of the participant</param>
        /// <param name="from">start date interval</param>
        /// <param name="to">end date interval</param>
        /// <param name="ct"></param>
        /// 
        /// 
        /// <returns>Paginated list of appointments</returns>
        /// <reponse code="400">
        ///     either <paramref name="id"/> is not set,
        ///     specified date interval is not valid (<paramref name="from"/> is greater than <paramref name="to"/>
        ///     <paramref name="page"/> is negative or 0
        ///     <paramref name="pageSize"/> is negative or 0
        /// </reponse>
        /// <reponse code="404">no participant with the specified <paramref name="id"/></reponse>
        [HttpHead("{id}/planning")]
        [HttpGet("{id}/planning")]
        [ProducesResponseType(typeof(IEnumerable<Browsable<AppointmentInfo>>), Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Browsable<AppointmentInfo>>>> Planning([RequireNonDefault] Guid id, DateTimeOffset from, DateTimeOffset to = default, CancellationToken ct = default)
        {
            IRequest<Option<IEnumerable<AppointmentInfo>>> query = new GetPlanningByParticipantIdQuery(id, from, to);

            Option<IEnumerable<AppointmentInfo>> optionalAppointments = await _mediator.Send(query, ct)
                .ConfigureAwait(false);
            ActionResult<IEnumerable<Browsable<AppointmentInfo>>> actionResult = new NotFoundResult();

            return optionalAppointments.Match(
                some: appointments => new ActionResult<IEnumerable<Browsable<AppointmentInfo>>>(
                    appointments.Select(resource => new Browsable<AppointmentInfo>
                    {
                        Resource = resource,
                        Links = new[]
                        {
                            new Link{ Relation = Self, Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = AppointmentsController.EndpointName, resource.Id })}
                        }
                    })),
                none: () => new NotFoundResult()
            );
        }

        /// <summary>
        /// Search `participants` resources based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <param name="ct">Notfies to cancel the search operation</param>
        /// <remarks>
        /// <para>
        ///     All criteria are combined as a AND.
        /// </para>
        /// <para>
        /// Advanded search :
        /// Several operators that can be used to make an advanced search :
        /// '*' : match zero or more characters in a string property.
        /// </para>
        /// <para>
        ///     // GET /participants/search?name=Wayne
        ///     will match all resources which have exactly 'Wayne' in the `name` property
        /// </para>
        /// <para>
        ///     // GET /participants/search?name=C*tral
        ///     will match match all resources where name starts with 'C' and ends with 'tral'.
        /// </para>
        /// <para>'?' : match exactly one charcter in a string property.</para>
        /// <para>'!' : negate a criterion</para>
        /// <para>
        ///     // GET /participants/search?name=!Wayne
        ///     will match all resources where `name` is not "Wayne"
        /// </para>
        ///     
        /// </remarks>
        /// <response code="200">"page" of resources that matches <paramref name="search"/> criteria.</response>
        /// <response code="404">"page" is greater than the number of pages in the result set.</response>
        /// <response code="400">one of the search criterion is not valid</response>
        [HttpGet("[action]")]
        [HttpHead("[action]")]
        [ProducesResponseType(typeof(GenericPagedGetResponse<Browsable<ParticipantInfo>>), Status200OK)]
        [ProducesResponseType(typeof(PageLinks), Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<ActionResult<GenericPagedGetResponse<Browsable<ParticipantInfo>>>> Search([FromQuery] SearchParticipantInfo search, CancellationToken ct = default)
        {
            search.PageSize = Math.Min(_apiOptions.Value.MaxPageSize, search.PageSize);

            Page<ParticipantInfo> page = await _mediator.Send(new SearchParticipantInfoQuery(search), ct)
                .ConfigureAwait(false);
            ActionResult<GenericPagedGetResponse<Browsable<ParticipantInfo>>> actionResult;
            string linkToFirstPage = _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.Name, search.Email, search.Sort, page = 1, search.PageSize });
            string linkToLastPage = _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.Name, search.Email, search.Sort, page = page.Count, search.PageSize });

            if (search.Page > page.Count)
            {
                PageLinks validLinks = new PageLinks(
                    first: linkToFirstPage,
                    previous: null,
                    next: null,
                    last: linkToLastPage);
                actionResult = new NotFoundObjectResult(validLinks);
            }
            else
            {
                actionResult = new GenericPagedGetResponse<Browsable<ParticipantInfo>>(
                    page.Entries.Select(x => new Browsable<ParticipantInfo>
                    {
                        Resource = x,
                        Links = new[]
                        {
                        new Link { Relation = Self, Method = "GET", Href =_urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { x.Id })}
                        }
                    }),
                    first: linkToFirstPage,
                    previous: page.Count > 1 && search.Page > 1
                        ? _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.Name, search.Email, search.Sort, page = search.Page - 1, search.PageSize })
                        : null,
                    next: search.Page < page.Count
                        ? _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = EndpointName, search.Name, search.Email, search.Sort, page = search.Page + 1, search.PageSize })
                        : null,
                    last: linkToLastPage,
                    total: page.Total
                );
            }

            return actionResult;
        }
    }
}
