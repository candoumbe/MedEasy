using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint that handle <see cref="PrescriptionHeaderInfo"/> resources.
    /// </summary>
    [Route("api/[controller]")]
    public class PrescriptionsController
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private object details;

        /// <summary>
        /// Builds a new <see cref="PrescriptionsController"/> instance
        /// </summary>
        /// <param name="logger">logger that will be used to log action flow</param>
        /// <param name="apiOptions">API options</param>
        /// <param name="prescriptionService">service to handle specific</param>
        /// <param name="actionContextAccessor">Gives access to the current <see cref="ActionContext"/></param>
        /// <param name="urlHelperFactory">Factory to builds urls</param>
        public PrescriptionsController(ILogger<PrescriptionsController> logger, IOptions<MedEasyApiOptions> apiOptions, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        /// <summary>
        /// Endpoint name
        /// </summary>
        public static string EndpointName => nameof(PrescriptionsController).Replace("Controller", string.Empty);


        /// <summary>
        /// Gets the <see cref="PrescriptionHeaderInfo"/> with the specified <paramref name="id"/>
        /// </summary>
        /// <param name="id">id of the prescription header to retrieve</param>
        /// <returns></returns>
        /// <response code="200">if a prescription is the specified <paramref name="id"/> is found</response>
        /// <response code="404">if no prescription found</response>
        [HttpGet("{id:int}")]
        [HttpHead("{id:int}")]
        [Produces(typeof(PrescriptionHeaderInfo))]
        public async Task<IActionResult> Get(int id)
        {
            IActionResult actionResult;
            PrescriptionHeaderInfo prescriptionHeaderInfo = await _prescriptionService.GetOnePrescriptionAsync(id);
            if (prescriptionHeaderInfo != null)
            {
                IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                IBrowsableResource<PrescriptionHeaderInfo> browsableResource = new BrowsableResource<PrescriptionHeaderInfo>
                {
                    Resource = prescriptionHeaderInfo,
                    Links = new[] {
                        new Link
                        {
                            Href = urlHelper.Action(nameof(Get), EndpointName, new { prescriptionHeaderInfo.Id }),
                            Rel = "self"
                        },
                        new Link
                        {
                            Rel = nameof(Prescription.Items),
                            Href = urlHelper.Action(nameof(PrescriptionsController.Details), EndpointName , new { prescriptionHeaderInfo.Id })
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
        /// <returns></returns>
        /// <response code="200">if the resource exists</response>
        /// <response code="404">if no prescription with <paramref name="id"/> found</response>
        [HttpGet("{id:int}/[action]")]
        [HttpHead("{id:int}/[action]")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionItemInfo>), 200)]
        public async Task<IActionResult> Details(int id)
        {
            IEnumerable<PrescriptionItemInfo> items = await _prescriptionService.GetItemsByPrescriptionIdAsync(id);

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            IBrowsableResource<IEnumerable<PrescriptionItemInfo>> browsableResource = new BrowsableResource<IEnumerable<PrescriptionItemInfo>>
            {
                Resource = items,
                Links = new[] {
                        new Link
                        {
                            Rel = "self",
                            Href = urlHelper.Action(nameof(Details), EndpointName, new { id })
                        }
                    }
            };

            return new OkObjectResult(browsableResource);
            
        }
    }
}
