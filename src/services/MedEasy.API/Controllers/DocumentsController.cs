using MedEasy.DTO;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedEasy.DAL.Repositories;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using MedEasy.Queries.Document;
using MedEasy.Handlers.Core.Document.Queries;
using System.Threading;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint that handles <see cref="DocumentMetadataInfo"/> resources.
    /// </summary>
    public class DocumentsController : RestReadControllerBase<Guid, DocumentMetadataInfo>
    {
        /// <summary>
        /// Builds a new <see cref="DocumentsController"/> instance.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="getByIdHandler">Gets one document by its <see cref="DocumentMetadataInfo.DocumentId"/></param>
        /// <param name="getManyQueryHandler"></param>
        /// <param name="urlHelperFactory"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="getContentByIdHandler"></param>
        public DocumentsController(
            ILogger<DocumentsController> logger, IOptionsSnapshot<MedEasyApiOptions> apiOptions,
            IHandleGetOneDocumentMetadataInfoByIdQuery getByIdHandler,
            IHandleGetManyDocumentsQuery getManyQueryHandler,
            IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, 
            IHandleGetOneDocumentInfoByIdQuery getContentByIdHandler
             ) : base(logger, apiOptions, getByIdHandler, getManyQueryHandler, urlHelperFactory, actionContextAccessor)
        {
            GetDocumentContentByIdHandler = getContentByIdHandler;
        }

        /// <summary>
        /// Name of the endpoint
        /// </summary>
        public static string EndpointName => nameof(DocumentsController).Replace("Controller", string.Empty);
        /// <summary>
        /// Name of the controller
        /// </summary>
        protected override string ControllerName => EndpointName;

        /// <summary>
        /// Gets a document by its id.
        /// </summary>
        public IHandleQueryAsync<Guid, Guid, DocumentInfo, IWantOneResource<Guid, Guid, DocumentInfo>> GetDocumentContentByIdHandler { get; }


        /// <summary>
        /// Gets all documents
        /// </summary>
        /// <param name="page">1-based index of the page of resources to get. </param>
        /// <param name="pageSize">Number of items to get</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DocumentMetadataInfo>), 200)]
        public async Task<IActionResult> Get([FromQuery]int page, [FromQuery]int pageSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            PaginationConfiguration pageConfig = new PaginationConfiguration
            {
                Page = page,
                PageSize = pageSize
            };
            IPagedResult<DocumentMetadataInfo> result = await GetAll(pageConfig, cancellationToken);

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && pageConfig.Page > 1;

            IUrlHelper urlHelper = UrlHelperFactory.GetUrlHelper(ActionContextAccessor.ActionContext);

            string firstPageUrl = urlHelper.Action(nameof(Get), ControllerName, new { PageSize = pageConfig.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = pageConfig.PageSize, Page = pageConfig.Page - 1 })
                    : null;

            string nextPageUrl = pageConfig.Page < result.PageCount
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = pageConfig.PageSize, Page = pageConfig.Page + 1 })
                    : null;
            string lastPageUrl = result.PageCount > 0
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = pageConfig.PageSize, Page = result.PageCount })
                    : null;


            IGenericPagedGetResponse<DocumentMetadataInfo> response = new GenericPagedGetResponse<DocumentMetadataInfo>(
                result.Entries,
                firstPageUrl,
                previousPageUrl,
                nextPageUrl,
                lastPageUrl,
                result.Total);


            return new OkObjectResult(response);
        }

        /// <summary>
        /// Gets the metadata of the document with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">id of the document to get metadata from</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="200">The metadata of the document.</response>
        /// <response code="404">if no document metadata found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DocumentMetadataInfo), 200)]
        public override async Task<IActionResult> Get([FromQuery]Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            DocumentMetadataInfo resource = await GetByIdQueryHandler.HandleAsync(new WantOneDocumentMetadataInfoByIdQuery(id), cancellationToken);
            IActionResult actionResult;
            if (resource == null)
            {
                actionResult = new NotFoundResult();
            }
            else
            {
                IUrlHelper urlHelper = UrlHelperFactory.GetUrlHelper(ActionContextAccessor.ActionContext);
                IBrowsableResource<DocumentMetadataInfo> browsableResource = new BrowsableResource<DocumentMetadataInfo>
                {
                    Resource = resource,
                    Links = BuildAdditionalLinksForResource(resource, urlHelper)
                };
                actionResult = new OkObjectResult(browsableResource);
            }

            return actionResult;

        }

        /// <summary>
        /// Gets the binary content of the document with the specified <paramref name="id"/>
        /// </summary>
        /// <param name="id">id of the document to get content from</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="200">The document content</response>
        /// <response code="404">The resource was not found</response>
        [HttpGet("{id}/[action]")]
        [ProducesResponseType(typeof(DocumentInfo), 200)]
        public async Task<IActionResult> File(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            DocumentInfo resource = await GetDocumentContentByIdHandler.HandleAsync(new GenericGetOneResourceByIdQuery<Guid, DocumentInfo>(id), cancellationToken);
            IActionResult actionResult;
            if (resource != null)
            {
                IUrlHelper urlHelper = UrlHelperFactory.GetUrlHelper(ActionContextAccessor.ActionContext);
                IBrowsableResource<DocumentInfo> browsableResource = new BrowsableResource<DocumentInfo>
                {
                    Resource = resource,
                    Links = new[]
                    {
                        new Link { Relation = "self", Href = urlHelper.Action(nameof(File), EndpointName, new { resource.Id }) },
                        new Link { Relation = "metadata", Href = urlHelper.Action(nameof(Get), EndpointName, new { resource.Id }) }
                    }
                };
                actionResult = new OkObjectResult(browsableResource);
            }
            else
            {
                actionResult = new NotFoundResult();
            }

            return actionResult;
        }

        /// <summary>
        /// Inherited
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="urlHelper"></param>
        /// <returns></returns>
        protected override IEnumerable<Link> BuildAdditionalLinksForResource(DocumentMetadataInfo resource, IUrlHelper urlHelper) 
            => new[]
            {
                new Link { Relation = "self", Href = urlHelper.Action(nameof(Get), EndpointName, new { resource.Id })},
                new Link { Relation = "file", Href = urlHelper.Action(nameof(File), EndpointName, new { resource.Id })}
            };
    }
}
