using System.Collections.Generic;
using System.Threading.Tasks;
using MedEasy.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MedEasy.DTO;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MedEasy.Objects;
using System;
using System.Threading;
using Optional;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint that handle <see cref="PrescriptionHeaderInfo"/> resources.
    /// </summary>
    [Route("api/[controller]")]
    public class PrescriptionsController
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly IUrlHelper _urlHelper;

        /// <summary>
        /// Builds a new <see cref="PrescriptionsController"/> instance
        /// </summary>
        /// <param name="logger">logger that will be used to log action flow</param>
        /// <param name="apiOptions">API options</param>
        /// <param name="prescriptionService">service to handle specific</param>
        /// <param name="urlHelper">Helper to build urls</param>
        public PrescriptionsController(ILogger<PrescriptionsController> logger, IOptions<MedEasyApiOptions> apiOptions, IUrlHelper urlHelper, IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
            _urlHelper = urlHelper;
        }

        /// <summary>
        /// Endpoint name
        /// </summary>
        public static string EndpointName => nameof(PrescriptionsController).Replace("Controller", string.Empty);


        /// <summary>
        /// Gets the <see cref="PrescriptionHeaderInfo"/> with the specified <paramref name="id"/>
        /// </summary>
        /// <param name="id">id of the prescription header to retrieve</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="200">if a prescription is the specified <paramref name="id"/> is found</response>
        /// <response code="404">if no prescription found</response>
        [HttpGet("{id}")]
        [HttpHead("{id}")]
        [Produces(typeof(PrescriptionHeaderInfo))]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default)
        {
            Option<PrescriptionHeaderInfo> prescriptionHeaderInfo = await _prescriptionService.GetOnePrescriptionAsync(id, cancellationToken);

            return prescriptionHeaderInfo.Match<IActionResult>(
             some: x =>
             {
                 IBrowsableResource<PrescriptionHeaderInfo> browsableResource = new BrowsableResource<PrescriptionHeaderInfo>
                 {
                     Resource = x,
                     Links = new[] {
                        new Link
                        {
                            Href = _urlHelper.Action(nameof(Get), EndpointName, new { x.Id }),
                            Relation = "self"
                        },
                        new Link
                        {
                            Relation = nameof(Prescription.Items),
                            Href = _urlHelper.Action(nameof(PrescriptionsController.Details), EndpointName , new { x.Id })
                        }
                    }
                 };

                 return new OkObjectResult(browsableResource);
             },
             none: () => new NotFoundResult());
        }


        /// <summary>
        /// Gets the  content of the <see cref="PrescriptionInfo"/> with the specified <paramref name="id"/>.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="id">id of the resource to get details.</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="404">if no prescription with <paramref name="id"/> found</response>
        [HttpGet("{id}/[action]")]
        [HttpHead("{id}/[action]")]
        [ProducesResponseType(typeof(BrowsableResource<IEnumerable<PrescriptionItemInfo>>), 200)]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            Option<IEnumerable<PrescriptionItemInfo>> items = await _prescriptionService.GetItemsByPrescriptionIdAsync(id, cancellationToken);

            return items.Match<IActionResult>(
                some: x =>
                {
                    IBrowsableResource<IEnumerable<PrescriptionItemInfo>> browsableResource = new BrowsableResource<IEnumerable<PrescriptionItemInfo>>
                    {
                        Resource = x,
                        Links = new[] 
                        {
                            new Link
                            {
                                Relation = "self",
                                Href = _urlHelper.Action(nameof(Details), EndpointName, new { id })
                            }
                        }
                    };

                    return new OkObjectResult(browsableResource);
                },
                none: () => new NotFoundResult());

        }
    }
}
