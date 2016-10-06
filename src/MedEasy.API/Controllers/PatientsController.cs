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
using MedEasy.Commands;

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
        private readonly IRunAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo, TemperatureInfo> _iRunAddNewTemperatureCommand;
        private readonly IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo> _iHandleGetOneTemperatureQuery;
        private readonly IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo> _iHandleGetOneBloodPressureQuery;
        private readonly IRunAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo, BloodPressureInfo> _iRunAddNewBloodPressureCommand;

        /// <summary>
        /// Builds a new <see cref="PatientsController"/> instance
        /// </summary>
        /// <param name="getByIdQueryHandler">Handler of GET one resource</param>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="getManyPatientQueryHandler">Handler of GET many <see cref="PatientInfo"/> resources</param>
        /// <param name="iRunCreatePatientCommand">Runner of CREATE resource command</param>
        /// <param name="iRunDeletePatientByIdCommand">Runner of DELETE resource command</param>
        /// <param name="logger">logger</param>
        /// <param name="urlHelperFactory">Factory used to build <see cref="IUrlHelper"/> instances.</param>
        /// <param name="iHandleGetOneTemperatureQuery">Handler for GET <see cref="TemperatureInfo"/>  </param>
        /// <param name="iHandleGetOneBloodPressureQuery">Handler for GET <see cref="BloodPressureInfo"/>  </param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="iRunAddNewTemperatureCommand">Runners that can process command to create new <see cref="TemperatureInfo"/></param>
        /// <param name="iRunAddNewBloodPressureCommand">Runners that can process command to create new <see cref="BloodPressureInfo"/></param>
        public PatientsController(ILogger<PatientsController> logger, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor, 
            IOptions<MedEasyApiOptions> apiOptions,
            IHandleGetOnePatientInfoByIdQuery getByIdQueryHandler,
            IHandleGetManyPatientInfosQuery getManyPatientQueryHandler,
            IRunCreatePatientCommand iRunCreatePatientCommand,
            IRunDeletePatientByIdCommand iRunDeletePatientByIdCommand,
            IRunAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo, TemperatureInfo> iRunAddNewTemperatureCommand,
            IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo> iHandleGetOneTemperatureQuery,
            IRunAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo, BloodPressureInfo> iRunAddNewBloodPressureCommand,
            IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo> iHandleGetOneBloodPressureQuery


            ) : base(logger, apiOptions, getByIdQueryHandler, getManyPatientQueryHandler, iRunCreatePatientCommand, urlHelperFactory, actionContextAccessor)
        { 
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _iRunCreatePatientCommand = iRunCreatePatientCommand;
            _iRunDeletePatientByIdCommand = iRunDeletePatientByIdCommand;
            _iRunAddNewTemperatureCommand = iRunAddNewTemperatureCommand;
            _iRunAddNewBloodPressureCommand = iRunAddNewBloodPressureCommand;
            _iHandleGetOneTemperatureQuery = iHandleGetOneTemperatureQuery;
            _iHandleGetOneBloodPressureQuery = iHandleGetOneBloodPressureQuery;

        }


        /// <summary>
        /// Gets all the resources of the endpoint
        /// </summary>
        /// <param name="page">index of the page of resources to get</param>
        /// <param name="pageSize">number of resources to return </param>
        /// <remarks>
        /// Resources are returned as pages. The <paramref name="pageSize"/> value is used has a hint by the server
        /// and there's no garanty that the size of page of result will be equal to the <paramref name="pageSize"/> set in the query.
        /// In particular, the number of resources on a page may be caped by the server.
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(GenericGetResponse<BrowsableResource<PatientInfo>>))]
        public async Task<IActionResult> Get(int page, int pageSize)
        {
            GenericGetQuery query = new GenericGetQuery {
                Page = page,
                PageSize = pageSize
            };
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


            IGetResponse<BrowsableResource<PatientInfo>> response = new GenericPagedGetResponse<BrowsableResource<PatientInfo>>(
                result.Entries.Select(x => 
                    new BrowsableResource<PatientInfo>
                    {
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
        [Produces(typeof(BrowsableResource<PatientInfo>))]
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
            BrowsableResource<PatientInfo> browsableResource = new BrowsableResource<PatientInfo>
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
        [Produces(typeof(BrowsableResource<PatientInfo>))]
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
            return new OkResult();
        }


        /// <summary>
        /// Create a new <see cref="TemperatureInfo"/> resource
        /// </summary>
        /// <param name="input">input to create the new resource</param>
        /// <returns>The created resource</returns>
        /// <see cref="IRunAddNewPhysiologicalMeasureCommand{TKey, TData, TOutput}"/>
        [HttpPost("{id:int}/[action]")]
        [Produces(typeof(BrowsableResource<TemperatureInfo>))]
        public async Task<IActionResult> Temperatures(CreateTemperatureInfo input)
        {
            TemperatureInfo output = await _iRunAddNewTemperatureCommand.RunAsync(new AddNewPhysiologicalMeasureCommand<CreateTemperatureInfo, TemperatureInfo>(input));
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            BrowsableResource<TemperatureInfo> resource = new BrowsableResource<TemperatureInfo>
            {
                Resource = output,
                Location = new Link
                {
                    Href = urlHelper.Action(nameof(Temperatures), EndpointName, new { id = output.PatientId, temperatureId = output.Id }),
                    Rel = "self",
                }
            };
            OkObjectResult result = new OkObjectResult(resource);


            return result;
        }

        /// <summary>
        /// Create a new <see cref="BloodPressureInfo"/> resource
        /// </summary>
        /// <param name="input">input to create the new resource</param>
        /// <returns>The created resource</returns>
        /// <see cref="IRunAddNewPhysiologicalMeasureCommand{TKey, TData, TOutput}"/>
        [HttpPost("{id:int}/[action]")]
        [Produces(typeof(BrowsableResource<BloodPressureInfo>))]
        public async Task<IActionResult> BloodPressures(CreateBloodPressureInfo input)
        {
            BloodPressureInfo output = await _iRunAddNewBloodPressureCommand.RunAsync(new AddNewPhysiologicalMeasureCommand<CreateBloodPressureInfo, BloodPressureInfo>(input));
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            BrowsableResource<BloodPressureInfo> resource = new BrowsableResource<BloodPressureInfo>
            {
                Resource = output,
                Location = new Link
                {
                    Href = urlHelper.Action(nameof(BloodPressures), EndpointName, new { id = output.PatientId, temperatureId = output.Id }),
                    Rel = "self",
                }
            };
            OkObjectResult result = new OkObjectResult(resource);


            return result;
        }

        /// <summary>
        /// Gets one mesure of temperature for the specified patient
        /// </summary>
        /// <param name="id">Id of the <see cref="PatientInfo"/>.</param>
        /// <param name="temperatureId">id of the <see cref="TemperatureInfo"/> to get</param>
        /// <returns></returns>
        [HttpGet("{id:int}/[action]/{temperatureId:int}")]
        [Produces(typeof(BrowsableResource<TemperatureInfo>))]
        public async Task<IActionResult> Temperatures(int id, int temperatureId)
        {
            TemperatureInfo output = await _iHandleGetOneTemperatureQuery.HandleAsync(new WantOnePhysiologicalMeasureQuery<TemperatureInfo>(id, temperatureId));
            IActionResult actionResult = null;

            if (output != null)
            {
                IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                actionResult = new OkObjectResult(new BrowsableResource<TemperatureInfo>
                {
                    Resource = output,
                    Location = new Link
                    {
                        Href = urlHelper.Action(nameof(Temperatures), EndpointName, new { id = output.PatientId, temperatureId = output.Id }),
                        Rel = "self"
                    }
                });
                
            }
            else
            {
                actionResult = new NotFoundResult();
            }

            return actionResult;
        }

        /// <summary>
        /// Gets one mesure of temperature for the specified patient
        /// </summary>
        /// <param name="id">Id of the <see cref="PatientInfo"/>.</param>
        /// <param name="bloodPressureId">id of the <see cref="BloodPressureInfo"/> to get</param>
        /// <returns></returns>
        [HttpGet("{id:int}/[action]/{bloodPressureId:int}")]
        [Produces(typeof(BrowsableResource<BloodPressureInfo>))]
        public async Task<IActionResult> BloodPressures(int id, int bloodPressureId)
        {
            BloodPressureInfo output = await _iHandleGetOneBloodPressureQuery.HandleAsync(new WantOnePhysiologicalMeasureQuery<BloodPressureInfo>(id, bloodPressureId));
            IActionResult actionResult = null;

            if (output != null)
            {
                IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                actionResult = new OkObjectResult(new BrowsableResource<BloodPressureInfo>
                {
                    Resource = output,
                    Location = new Link
                    {
                        Href = urlHelper.Action(nameof(BloodPressures), EndpointName, new { id = output.PatientId, temperatureId = output.Id }),
                        Rel = "self"
                    }
                });

            }
            else
            {
                actionResult = new NotFoundResult();
            }

            return actionResult;
        }

    }
}
