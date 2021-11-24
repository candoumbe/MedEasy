namespace Documents.API.Features.v1
{
    using DataFilters;

    using Documents.CQRS.Commands;
    using Documents.CQRS.Queries;
    using Documents.DTO;
    using Documents.DTO.v1;
    using Documents.Ids;

    using MedEasy.CQRS.Core.Commands;
    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Repositories;
    using MedEasy.DTO;
    using MedEasy.DTO.Search;
    using MedEasy.RestObjects;

    using MediatR;

    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using static MedEasy.RestObjects.LinkRelation;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    /// <summary>
    /// Controller that handles <see cref="DocumentInfo"/>s
    /// </summary>
    [ApiController]
    [Route("v{v:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ProducesResponseType(Status401Unauthorized)]
    public class DocumentsController
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>
        public static string EndpointName => nameof(DocumentsController)
            .Replace(nameof(Controller), string.Empty);

        private readonly LinkGenerator _urlHelper;
        private readonly IOptionsSnapshot<DocumentsApiOptions> _apiOptions;
        private readonly IMediator _mediator;
        private readonly ILogger<DocumentsController> _logger;
        private readonly ApiVersion _apiVersion;

        /// <summary>
        /// Builds a new <see cref="DocumentsController"/> instance.
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="apiOptions"></param>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        /// <param name="apiVersion"></param>
        public DocumentsController(LinkGenerator urlHelper,
                                   IOptionsSnapshot<DocumentsApiOptions> apiOptions,
                                   IMediator mediator,
                                   ILogger<DocumentsController> logger,
                                   ApiVersion apiVersion
            )
        {
            _urlHelper = urlHelper;
            _apiOptions = apiOptions;
            _mediator = mediator;
            _logger = logger;
            _apiVersion = apiVersion;
        }

        /// <summary>
        /// Gets a subset of documents resources
        /// </summary>
        /// <param name="paginationConfiguration">paging configuration</param>
        /// <param name="ct">Notification to abort request execution</param>
        /// <returns></returns>
        /// <response code="200">A collection of resource</response>
        /// <response code="400">page or pageSize is negative or zero</response>
        [HttpGet]
        [HttpHead]
        [ProducesResponseType(typeof(GenericPagedGetResponse<Browsable<DocumentInfo>>), Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> Get([FromQuery] PaginationConfiguration paginationConfiguration, CancellationToken ct = default)
        {
            DocumentsApiOptions apiOptions = _apiOptions.Value;
            paginationConfiguration.PageSize = Math.Min(paginationConfiguration.PageSize, apiOptions.MaxPageSize);

            GetPageOfDocumentInfoQuery query = new(paginationConfiguration);

            Page<DocumentInfo> page = await _mediator.Send(query, ct)
                .ConfigureAwait(false);

            string version = _apiVersion.ToString();
            GenericPagedGetResponse<Browsable<DocumentInfo>> result = new(
                page.Entries.Select(resource => new Browsable<DocumentInfo>
                {
                    Resource = resource,
                    Links = new[]
                    {
                        new Link
                        {
                            Relation = Self,
                            Title = resource.Name,
                            Method = "GET",
                            Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller= EndpointName, resource.Id, version })
                        }
                    }
                }),

                first: _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = 1, paginationConfiguration.PageSize, version }),
                previous: paginationConfiguration.Page > 1 && page.Count > 1
                    ? _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = paginationConfiguration.Page - 1, paginationConfiguration.PageSize, version })
                    : null,
                next: paginationConfiguration.Page < page.Count
                    ? _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = paginationConfiguration.Page + 1, paginationConfiguration.PageSize, version })
                    : null,
                last: _urlHelper.GetPathByName(RouteNames.DefaultGetAllApi, new { controller = EndpointName, page = page.Count, paginationConfiguration.PageSize, version }),
                total: page.Total
            );

            return new OkObjectResult(result);
        }

        /// <summary>
        /// Delete the account with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">id of the resource to delete.</param>
        /// <param name="ct">Notifies to abort the request execution.</param>
        /// <returns></returns>
        /// <response code="204">The resource was successfully deleted.</response>
        /// <response code="404">Resource to delete was not found</response>
        /// <response code="409">Resource cannot be deleted</response>
        /// <response code="403"></response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(DocumentId id, CancellationToken ct = default)
        {
            DeleteDocumentInfoByIdCommand cmd = new(id);
            DeleteCommandResult cmdResult = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);

            return cmdResult switch
            {
                DeleteCommandResult.Done => new NoContentResult(),
                DeleteCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                DeleteCommandResult.Failed_NotFound => new NotFoundResult(),
                DeleteCommandResult.Failed_Conflict => new StatusCodeResult(Status409Conflict),
                _ => throw new ArgumentOutOfRangeException($"Unexpected <{cmdResult}> result"),
            };
        }

        /// <summary>
        /// Delete the specified account
        /// </summary>
        /// <param name="id">id of the account to delete</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [HttpHead("{id}")]
        public async Task<ActionResult<Browsable<DocumentInfo>>> Get([FromQuery] DocumentId id, CancellationToken ct = default)
        {
            Option<DocumentInfo> optionalDocument = await _mediator.Send(new GetOneDocumentInfoByIdQuery(id), ct)
                                                                   .ConfigureAwait(false);

            return optionalDocument.Match<ActionResult<Browsable<DocumentInfo>>>(
                some: document =>
                {
                    string version = _apiVersion.ToString();
                    IList<Link> links = new List<Link>
                    {
                        new Link { Relation = Self, Method = "GET", Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, document.Id, version }) },
                        new Link { Relation = "delete",Method = "DELETE", Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, document.Id, version }) },
                        new Link { Relation = "file", Method = "GET", Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, document.Id, action=nameof(File), version }) }
                    };

                    return new Browsable<DocumentInfo>
                    {
                        Resource = document,
                        Links = links
                    };
                },
                none: () => new NotFoundResult()
            );
        }

        /// <summary>
        /// Download the file associated with the document
        /// </summary>
        /// <param name="id">id of the document to download</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{id}/[action]")]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public async IAsyncEnumerable<DocumentPartInfo> File(DocumentId id, [EnumeratorCancellation] CancellationToken ct = default)
        {
            IAsyncEnumerable<DocumentPartInfo> parts = await _mediator.Send(new GetOneDocumentFileInfoByIdQuery(id), ct)
                                                                      .ConfigureAwait(false);

            await foreach (DocumentPartInfo chunk in parts.WithCancellation(ct))
            {
                yield return chunk;
            }
        }

        /// <summary>
        /// Partially update an account resource.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        /// </para>
        /// <para>    // PATCH api/documents/3594c436-8595-444d-9e6b-2686c4904725</para>
        /// <para>
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/Name",
        ///             "from": "string",
        ///             "value": "new file name.jpg"
        ///       }
        ///     ]
        /// </para>
        /// <para>The set of changes to apply will be applied atomically. </para>
        /// 
        /// </remarks>
        /// <param name="id">id of the resource to update.</param>
        /// <param name="changes">set of changes to apply to the resource.</param>
        /// <param name="ct">Notifies lower layers about the request abortion</param>
        /// <response code="204">The resource was successfully patched.</response>
        /// <response code="400">Changes are not valid for the selected resource.</response>
        /// <response code="404">Resource to "PATCH" not found</response>
        /// <response code="409">One or more patch operations would result in a invalid state</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ProblemDetails), Status400BadRequest)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status409Conflict)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Patch(Guid id, [BindRequired, FromBody] JsonPatchDocument<DocumentInfo> changes, CancellationToken ct = default)
        {
            PatchInfo<Guid, DocumentInfo> data = new()
            {
                Id = id,
                PatchDocument = changes
            };
            PatchCommand<Guid, DocumentInfo> cmd = new(data);

            ModifyCommandResult cmdResult = await _mediator.Send(cmd, ct)
                                                           .ConfigureAwait(false);

            return cmdResult switch
            {
                ModifyCommandResult.Done => new NoContentResult(),
                ModifyCommandResult.Failed_Unauthorized => new UnauthorizedResult(),
                ModifyCommandResult.Failed_NotFound => new NotFoundResult(),
                ModifyCommandResult.Failed_Conflict => new StatusCodeResult(Status409Conflict),
                _ => throw new ArgumentOutOfRangeException($"Unexpected <{cmdResult}> patch result"),
            };
        }

        /// <summary>
        /// Creates an account resource.
        /// </summary>
        /// <param name="newDocument">Data of the new account</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <response code="201">The resource was  created successfully.</response>
        /// <response code="400">Changes are not valid for the selected resource.</response>
        /// <response code="409">A document with the same <see cref="DocumentInfo.Hash"/> already exists.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Browsable<DocumentInfo>), Status201Created)]
        public async Task<IActionResult> Post([FromBody] NewDocumentInfo newDocument, CancellationToken ct = default)
        {
            CreateDocumentInfoCommand cmd = new(newDocument);

            Option<DocumentInfo, CreateCommandResult> optionalDocument = await _mediator.Send(cmd, ct)
                .ConfigureAwait(false);

            return optionalDocument.Match<IActionResult>(
                some: doc =>
                {
                    string version = _apiVersion?.ToString() ?? "1.0";
                    Browsable<DocumentInfo> browsableResource = new()
                    {
                        Resource = doc,
                        Links = new[]
                        {
                            new Link
                            {
                                Relation = Self,
                                Method = "GET",
                                Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new {version,  controller = EndpointName, doc.Id})
                            }
                        }
                    };

                    return new CreatedAtRouteResult(RouteNames.DefaultGetOneByIdApi, new { controller = EndpointName, doc.Id, version }, browsableResource);
                },
                none: cmdError =>
                {
                    return cmdError switch
                    {
                        CreateCommandResult.Failed_Conflict => new StatusCodeResult(Status409Conflict),
                        _ => throw new ArgumentOutOfRangeException($"Unexpected <{cmdError}> result when creating an account"),
                    };
                });
        }

        /// <summary>
        /// Search for documents
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("search")]
        [HttpHead("search")]
        public async Task<IActionResult> Search([BindRequired, FromQuery] SearchDocumentInfo search, CancellationToken ct = default)
        {
            search.PageSize = Math.Min(search.PageSize, _apiOptions.Value.MaxPageSize);
            IList<IFilter> filters = new List<IFilter>();

            if (!string.IsNullOrWhiteSpace(search.Name))
            {
                filters.Add($"{nameof(DocumentInfo.Name)}={search.Name}".ToFilter<DocumentInfo>());
            }
            if (!string.IsNullOrWhiteSpace(search.MimeType))
            {
                filters.Add($"{nameof(DocumentInfo.MimeType)}={search.MimeType}".ToFilter<DocumentInfo>());
            }

            SearchQueryInfo<DocumentInfo> searchQuery = new()
            {
                Page = search.Page,
                PageSize = search.PageSize,
                Filter = filters.Skip(1).Any()
                    ? new MultiFilter { Logic = FilterLogic.And, Filters = filters }
                    : filters.Single(),

                Sort = search.Sort?.ToSort<DocumentInfo>() ?? new Sort<DocumentInfo>(nameof(DocumentInfo.UpdatedDate), SortDirection.Descending)
            };

            Page<DocumentInfo> searchResult = await _mediator.Send(new SearchQuery<DocumentInfo>(searchQuery), ct)
                                                             .ConfigureAwait(false);

            bool hasNextPage = search.Page < searchResult.Count;
            string version = _apiVersion.ToString();
            return new OkObjectResult(new GenericPagedGetResponse<Browsable<DocumentInfo>>(

                items: searchResult.Entries.Select(x =>
                {
                    return new Browsable<DocumentInfo>
                    {
                        Resource = x,
                        Links = new[]
                        {
                            new Link { Relation = Self, Method = "GET", Href = _urlHelper.GetPathByName(RouteNames.DefaultGetOneByIdApi, new { x.Id, version } ) }
                        }
                    };
                }),
                first: _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                                new { page = 1, search.PageSize, search.Name, search.Sort, search.MimeType, controller = EndpointName, version }),
                next: hasNextPage
                    ? _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                               new { page = search.Page + 1, search.PageSize, search.Name, search.MimeType, search.Sort, controller = EndpointName, version })
                    : null,
                last: _urlHelper.GetPathByName(RouteNames.DefaultSearchResourcesApi,
                                               new { page = searchResult.Count, search.PageSize, search.Name, search.MimeType, search.Sort, controller = EndpointName, version }),
                total: searchResult.Total
            ));
        }
    }
}
