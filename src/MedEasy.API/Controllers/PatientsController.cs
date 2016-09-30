using System;
using System.Threading.Tasks;
using MedEasy.Objects;
using MedEasy.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MedEasy.DAL.Repositories;
using System.Linq;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Queries.Patient;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.Commands.Patient;
using Microsoft.Extensions.Options;
using Swashbuckle.SwaggerGen.Annotations;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="PatientInfo"/> resources
    /// </summary>
    [Route("api/[controller]")]
    public class PatientsController : RestCRUDControllerBase<int, Patient, PatientInfo, IWantOnePatientInfoByIdQuery, IWantManyPatientInfoQuery, Guid, CreatePatientInfo, ICreatePatientCommand, IRunCreatePatientCommand>
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>
        public static string EndpointName => nameof(PatientsController).Replace("Controller", string.Empty);


        /// <summary>
        /// Name of the controller without the "Controller" suffix 
        /// </summary>
        protected override string ControllerName => EndpointName;

       

        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IRunCreatePatientCommand _iRunCreatePatientCommand;
        private readonly IRunDeletePatientByIdCommand _iRunDeletePatientByIdCommand;

        /// <summary>
        /// Builds a new <see cref="PatientsController"/> instance
        /// </summary>
        /// <param name="getByIdQueryHandler">Handler of GET one resource</param>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="getManyPatientQueryHandler">Handler of GET many resources</param>
        /// <param name="iRunCreatePatientCommand">Runner of CREATE resource command</param>
        /// <param name="iRunDeletePatientByIdCommand">Runner of DELETE resource command</param>
        /// <param name="logger">logger</param>
        /// <param name="urlHelperFactory">Factory used to build <see cref="IUrlHelper"/> instances.</param>
        /// <param name="actionContextAccessor"></param>
        public PatientsController(ILogger<PatientsController> logger, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor, 
            IOptions<MedEasyApiOptions> apiOptions,
            IHandleGetOnePatientInfoByIdQuery getByIdQueryHandler,
            IHandleGetManyPatientInfosQuery getManyPatientQueryHandler,
            IRunCreatePatientCommand iRunCreatePatientCommand,
            IRunDeletePatientByIdCommand iRunDeletePatientByIdCommand) : base(logger, apiOptions, getByIdQueryHandler, getManyPatientQueryHandler, iRunCreatePatientCommand, urlHelperFactory, actionContextAccessor)
        { 
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _iRunCreatePatientCommand = iRunCreatePatientCommand;
            _iRunDeletePatientByIdCommand = iRunDeletePatientByIdCommand;

        }


        /// <summary>
        /// Gets all the entries in the repository
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(GenericGetResponse<BrowsablePatientInfo>))]
        public async Task<IActionResult> Get(GenericGetQuery query)
        {
            if (query == null)
            {
                query = new GenericGetQuery();
            }

            
                
            IPagedResult<PatientInfo> result = await GetAll(query);
           
            
            int count = result.Entries.Count();
             
            bool hasPreviousPage = count > 0 && query.Page > 1;

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);


            string firstPageUrl = urlHelper.Action(nameof(Get), ControllerName, new {PageSize = query.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = query.Page - 1 })
                    : null;

            string nextPageUrl = query.Page < result.PageCount
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = query.Page + 1 })
                    : null;
            string lastPageUrl = result.PageCount > 0
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = result.PageCount })
                    : null;


            IGetResponse<BrowsablePatientInfo> response = new GenericPagedGetResponse<BrowsablePatientInfo>(
                result.Entries.Select(x => 
                    new BrowsablePatientInfo {
                        Location = new Link { Href = urlHelper.Action(nameof(PatientsController.Get), ControllerName, new { Id = x.Id }) },
                        Resource = x
                    }),
                firstPageUrl,
                previousPageUrl,
                nextPageUrl,
                lastPageUrl,
                result.Total);
            

            return new OkObjectResult(response);
        }


        /// <summary>
        /// Gets the <see cref="PatientInfo"/> resource by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to look for</param>
        /// <returns></returns>
        [HttpHead("{id:int}")]
        [HttpGet("{id:int}")]
        [Produces(typeof(BrowsablePatientInfo))]
        public async override Task<IActionResult> Get(int id) => await base.Get(id);
            

        
        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="info">data used to create the resource</param>
        /// <returns>the created resource</returns>
        /// <response code="200">the resource was created successfuyl</response>
        [HttpPost]
        [Produces(typeof(BrowsableResource<PatientInfo>))]
        
        public async Task<IActionResult> Post([FromBody] CreatePatientInfo info)
        {
            PatientInfo output = await _iRunCreatePatientCommand.RunAsync(new CreatePatientCommand(info));
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            BrowsablePatientInfo browsableResource = new BrowsablePatientInfo
            {
                Resource = output,
                Location = new Link
                {
                    Href = urlHelper.Action(nameof(Get), ControllerName, new { id = output.Id }),
                    Rel = "self"
                }
            };


            return new OkObjectResult(browsableResource);
        }

        
        /// <summary>
        /// Updates the specified resource
        /// </summary>
        /// <param name="id">identifier of the resource to update</param>
        /// <param name="info">new values to set</param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        [Produces(typeof(BrowsablePatientInfo))]
        public async Task<IActionResult> Put(int id, [FromBody] PatientInfo info)
        {
            throw new NotImplementedException();
        }

        // DELETE api/specialties/5

        /// <summary>
        /// Delete the <see cref="PatientInfo"/> by its 
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _iRunDeletePatientByIdCommand.RunAsync(new DeletePatientByIdCommand(id));
            return await Task.FromResult(new OkResult());
        }




    }
}
