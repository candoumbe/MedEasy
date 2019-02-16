using Agenda.API.Routing;
using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DTO;
using MedEasy.Core.Attributes;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agenda.API.Resources
{
    [Route("agenda/[controller]")]
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
                    Links =new[]
                    {
                        new Link
                        {
                            Relation = LinkRelation.Self,
                            Method = "GET",
                            Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, x.Id })
                        }
                    }
                }),
                first: _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = 1, pageSize }),
                previous : result.Count > 2 && page > 1
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new {controller = EndpointName, page = page - 1, pageSize })
                    :  null,
                next: page < result.Count
                    ? _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = page + 1, pageSize })
                    : null,

                last: _urlHelper.Link(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = result.Count, pageSize }),
                count : result.Total
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
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
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
                                Relation = LinkRelation.Self,
                                Method = "GET",
                                Href = _urlHelper.Link(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, resource.Id })
                            },
                        }
                    };

                    return new OkObjectResult(resource);
                },

                none: () => new NotFoundResult());
        }
    }
}
