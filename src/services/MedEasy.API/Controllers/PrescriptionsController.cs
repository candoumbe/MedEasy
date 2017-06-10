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
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            IActionResult actionResult;
            PrescriptionHeaderInfo prescriptionHeaderInfo = await _prescriptionService.GetOnePrescriptionAsync(id, cancellationToken);
            if (prescriptionHeaderInfo != null)
            {
                IBrowsableResource<PrescriptionHeaderInfo> browsableResource = new BrowsableResource<PrescriptionHeaderInfo>
                {
                    Resource = prescriptionHeaderInfo,
                    Links = new[] {
                        new Link
                        {
                            Href = _urlHelper.Action(nameof(Get), EndpointName, new { prescriptionHeaderInfo.Id }),
                            Relation = "self"
                        },
                        new Link
                        {
                            Relation = nameof(Prescription.Items),
                            Href = _urlHelper.Action(nameof(PrescriptionsController.Details), EndpointName , new { prescriptionHeaderInfo.Id })
                        }
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
        /// Gets the  content of the <see cref="PrescriptionInfo"/> with the specified <paramref name="id"/>.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="id">id of the resource to get details.</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="200">if the resource exists</response>
        /// <response code="404">if no prescription with <paramref name="id"/> found</response>
        [HttpGet("{id}/[action]")]
        [HttpHead("{id}/[action]")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionItemInfo>), 200)]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<PrescriptionItemInfo> items = await _prescriptionService.GetItemsByPrescriptionIdAsync(id, cancellationToken);

            IBrowsableResource<IEnumerable<PrescriptionItemInfo>> browsableResource = new BrowsableResource<IEnumerable<PrescriptionItemInfo>>
            {
                Resource = items,
                Links = new[] {
                        new Link
                        {
                            Relation = "self",
                            Href = _urlHelper.Action(nameof(Details), EndpointName, new { id })
                        }
                    }
            };

            return new OkObjectResult(browsableResource);
            
        }
    }
}
