using AutoMapper;
using MedEasy.API.Models;
using MedEasy.Commands;
using MedEasy.Commands.Patient;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.DTO;
using MedEasy.Handlers;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Objects;
using MedEasy.Queries.Patient;
using MedEasy.Queries.Prescriptions;
using MedEasy.Queries.Search;
using MedEasy.RestObjects;
using MedEasy.Services;
using MedEasy.Validators;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static MedEasy.Data.DataFilterLogic;

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

        private readonly IPhysiologicalMeasureService _physiologicalMeasureService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly IRunPatchPatientCommand _iRunPatchPatientCommmand;
        private readonly IMapper _mapper;
        private readonly IHandleSearchQuery _iHandleSearchQuery;
        private readonly IHandleGetDocumentsByPatientIdQuery _iHandleGetDocumentByPatientIdQuery;

        /// <summary>
        /// Builds a new <see cref="PatientsController"/> instance
        /// </summary>
        /// <param name="getByIdQueryHandler">Handler of GET one resource</param>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="getManyPatientQueryHandler">Handler of GET many <see cref="PatientInfo"/> resources</param>
        /// <param name="iRunCreatePatientCommand">Runner of CREATE resource command</param>
        /// <param name="iRunDeletePatientByIdCommand">Runner of DELETE resource command</param>
        /// <param name="logger">logger</param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="iHandleSearchQuery">handler for <see cref="SearchQueryInfo{T}"/></param>
        /// <param name="urlHelperFactory">Factory used to build <see cref="IUrlHelper"/> instances.</param>
        /// <param name="physiologicalMeasureService">Service that deals with everything that's related to <see cref="PhysiologicalMeasurementInfo"/> resources</param>
        /// <param name="prescriptionService">Service that deals with everything that's related to <see cref="PrescriptionInfo"/> resources</param>
        /// <param name="iRunPatchPatientCommmand">Runner for changing main doctor ID command.</param>
        /// <param name="iHandleGetDocumentByPatientIdQuery">Handler for retrieving patient's <see cref="DocumentMetadataInfo"/>s.</param>
        /// <param name="mapper">Mapper to convert one type to an other.</param>
        public PatientsController(ILogger<PatientsController> logger, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IOptions<MedEasyApiOptions> apiOptions,
            IHandleSearchQuery iHandleSearchQuery,
            IHandleGetOnePatientInfoByIdQuery getByIdQueryHandler,
            IHandleGetManyPatientInfosQuery getManyPatientQueryHandler,
            IRunCreatePatientCommand iRunCreatePatientCommand,
            IRunDeletePatientByIdCommand iRunDeletePatientByIdCommand,
            IPhysiologicalMeasureService physiologicalMeasureService,
            IPrescriptionService prescriptionService,
            IHandleGetDocumentsByPatientIdQuery iHandleGetDocumentByPatientIdQuery,
            IRunPatchPatientCommand iRunPatchPatientCommmand, IMapper mapper
            ) : base(logger, apiOptions, getByIdQueryHandler, getManyPatientQueryHandler, iRunCreatePatientCommand, urlHelperFactory, actionContextAccessor)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _iRunCreatePatientCommand = iRunCreatePatientCommand;
            _iRunDeletePatientByIdCommand = iRunDeletePatientByIdCommand;
            _physiologicalMeasureService = physiologicalMeasureService;
            _prescriptionService = prescriptionService;
            _iRunPatchPatientCommmand = iRunPatchPatientCommmand;
            _iHandleSearchQuery = iHandleSearchQuery;
            _iHandleGetDocumentByPatientIdQuery = iHandleGetDocumentByPatientIdQuery;
            _mapper = mapper;
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
        /// <response code="200">items of the page</response>
        /// <response code="400"><paramref name="page"/> or <paramref name="pageSize"/> is negative or zero</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PatientInfo>), 200)]
        public async Task<IActionResult> Get([FromQuery] int page, [FromQuery] int pageSize)
        {
            GenericGetQuery query = new GenericGetQuery
            {
                Page = page,
                PageSize = pageSize
            };
            IPagedResult<PatientInfo> result = await GetAll(query);

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && query.Page > 1;

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            string firstPageUrl = urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = query.Page - 1 })
                    : null;

            string nextPageUrl = query.Page < result.PageCount
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = query.Page + 1 })
                    : null;
            string lastPageUrl = result.PageCount > 0
                    ? urlHelper.Action(nameof(Get), ControllerName, new { PageSize = query.PageSize, Page = result.PageCount })
                    : null;


            IGenericPagedGetResponse<PatientInfo> response = new GenericPagedGetResponse<PatientInfo>(
                result.Entries,
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
        [ProducesResponseType(typeof(PatientInfo), 200)]
        public async override Task<IActionResult> Get(int id) => await base.Get(id);

        /// <summary>
        /// Creates a new <see cref="PatientInfo"/> resource.
        /// </summary>
        /// <param name="newPatient">data used to create the resource</param>
        /// <response code="201">the resource was created successfully</response>
        /// <response code="400"><paramref name="newPatient"/> is not valid</response>
        [HttpPost]
        [ProducesResponseType(typeof(PatientInfo), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Post([FromBody] CreatePatientInfo newPatient)
        {
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            PatientInfo resource = await _iRunCreatePatientCommand.RunAsync(new CreatePatientCommand(newPatient));
            IBrowsableResource<PatientInfo> browsableResource = new BrowsableResource<PatientInfo>
            {
                Resource = resource,
                Links = BuildAdditionalLinksForResource(resource, urlHelper)
            };
            return new CreatedAtActionResult(nameof(Get), EndpointName, new { resource.Id }, browsableResource);
        }


        /// <summary>
        /// Updates the specified resource
        /// </summary>
        /// <param name="id">identifier of the resource to update</param>
        /// <param name="info">new values to set</param>
        /// <returns></returns>
        /// <response code="200">the operation succeed</response>
        /// <response code="400">Submitted values contains an error</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(PatientInfo), 200)]
        public async Task<IActionResult> Put([Range(1, int.MaxValue)] int id, [FromBody] CreatePatientInfo info)
        {
            throw new NotImplementedException();
        }

        // DELETE api/patients/5

        /// <summary>
        /// Delete the <see cref="PatientInfo"/> by its 
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <returns></returns>
        /// <response code="200">if the operation succeed</response>
        /// <response code="400">if <paramref name="id"/> is negative or zero</response>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([Range(1, int.MaxValue)] int id)
        {
            IActionResult actionResult;
            if (id <= 0)
            {
                actionResult = new BadRequestResult();
            }
            else
            {
                await _iRunDeletePatientByIdCommand.RunAsync(new DeletePatientByIdCommand(id));
                actionResult = new OkResult();
            }

            return actionResult;
        }


        /// <summary>
        /// Create a new <see cref="TemperatureInfo"/> resource.
        /// </summary>
        /// <param name="id">id of the patient the new measure will be attached to</param>
        /// <param name="newTemperature">input to create the new resource</param>
        /// <see cref="IPhysiologicalMeasureService.AddNewMeasureAsync{TPhysiologicalMeasure, TPhysiologicalMeasureInfo}(ICommand{Guid, TPhysiologicalMeasure})"/>
        /// <response code="201">if the creation succeed</response>
        /// <response code="400"><paramref name="newTemperature"/> is not valid or <paramref name="id"/> is negoative or zero</response>.
        [HttpPost("{id:int}/[action]")]
        [ProducesResponseType(typeof(TemperatureInfo), 201)]
        [ProducesResponseType(typeof(ModelStateDictionary), 400)]
        public async Task<IActionResult> Temperatures(int id, [FromBody] CreateTemperatureInfo newTemperature)
        {
            Temperature newMeasure = new Temperature
            {
                PatientId = id,
                DateOfMeasure = newTemperature.DateOfMeasure,
                Value = newTemperature.Value
            };
            TemperatureInfo output = await _physiologicalMeasureService
                .AddNewMeasureAsync<Temperature, TemperatureInfo>(new AddNewPhysiologicalMeasureCommand<Temperature, TemperatureInfo>(newMeasure));
            return new CreatedAtActionResult(nameof(Temperatures), EndpointName, new { id = output.PatientId, temperatureId = output.Id }, output);
        }

        /// <summary>
        /// Create a new <see cref="BloodPressureInfo"/> resource
        /// </summary>
        /// <remarks>
        /// <see cref="CreateBloodPressureInfo.SystolicPressure"/> and <see cref="CreateBloodPressureInfo.DiastolicPressure"/> values must be expressed in 
        ///  millimeters of mercury (mmHg)
        /// </remarks>
        /// <param name="id">id of the patient the new blood pressure will be attached to</param>
        /// <param name="newBloodPressure">input to create the new resource</param>
        /// <response code="201">the resource creation succeed</response>
        /// <response code="400"><paramref name="newBloodPressure"/> is not valid or <paramref name="id"/> is negative or zero</response>
        [HttpPost("{id:int}/[action]")]
        [ProducesResponseType(typeof(BloodPressureInfo), 200)]
        public async Task<IActionResult> BloodPressures(int id, [FromBody] CreateBloodPressureInfo newBloodPressure)
        {
            BloodPressure newMeasure = new BloodPressure
            {
                PatientId = id,
                DateOfMeasure = newBloodPressure.DateOfMeasure,
                SystolicPressure = newBloodPressure.SystolicPressure,
                DiastolicPressure = newBloodPressure.DiastolicPressure
            };
            BloodPressureInfo output = await _physiologicalMeasureService.AddNewMeasureAsync<BloodPressure, BloodPressureInfo>(new AddNewPhysiologicalMeasureCommand<BloodPressure, BloodPressureInfo>(newMeasure));
            return new CreatedAtActionResult(nameof(BloodPressures), EndpointName, new { id = output.PatientId, bloodPressureId = output.Id }, output);
        }

        /// <summary>
        /// Gets one mesure of temperature for the specified patient
        /// </summary>
        /// <param name="id">Id of the <see cref="PatientInfo"/>.</param>
        /// <param name="temperatureId">id of the <see cref="TemperatureInfo"/> to get</param>
        /// <returns></returns>
        [HttpGet("{id:int}/[action]/{temperatureId:int}")]
        [HttpHead("{id:int}/[action]/{temperatureId:int}")]
        [ProducesResponseType(typeof(TemperatureInfo), 200)]
        [ProducesResponseType(typeof(ModelStateDictionary), 400)]
        public async Task<IActionResult> Temperatures(int id, int temperatureId)
        {
            TemperatureInfo output = await _physiologicalMeasureService.GetOneMeasureAsync<Temperature, TemperatureInfo>(new WantOnePhysiologicalMeasureQuery<TemperatureInfo>(id, temperatureId));
            IActionResult actionResult = null;

            if (output != null)
            {
                IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                actionResult = new OkObjectResult(new BrowsableResource<TemperatureInfo>
                {
                    Resource = output,
                    Links = new[] {
                        new Link { Href = urlHelper.Action(nameof(Temperatures), EndpointName, new { id = output.PatientId, temperatureId = output.Id }), Rel = "self", Method = "GET"},
                        new Link { Href = urlHelper.Action(nameof(Temperatures), EndpointName, new { output.Id }), Rel = "remove", Method = "DELETE" },
                        new Link { Href = urlHelper.Action(nameof(Temperatures), EndpointName, new { output.Id }), Rel = "direct-link", Method = "GET" }
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
        [HttpHead("{id:int}/[action]/{bloodPressureId:int}")]
        [ProducesResponseType(typeof(BloodPressureInfo), 200)]
        public async Task<IActionResult> BloodPressures([Range(1, int.MaxValue)] int id, [Range(1, int.MaxValue)] int bloodPressureId)
        {
            BloodPressureInfo output = await _physiologicalMeasureService.GetOneMeasureAsync<BloodPressure, BloodPressureInfo>(new WantOnePhysiologicalMeasureQuery<BloodPressureInfo>(id, bloodPressureId));
            IActionResult actionResult = null;

            if (output != null)
            {
                IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                actionResult = new OkObjectResult(new BrowsableResource<BloodPressureInfo>
                {
                    Resource = output,
                    Links = new[] {
                        new Link
                        {
                            Href = urlHelper.Action(nameof(BloodPressures), EndpointName, new { id = output.PatientId, temperatureId = output.Id }),
                            Rel = "self"
                        }
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
        /// Get the last <see cref="BloodPressureInfo"/>
        /// </summary>
        /// <remarks>
        /// Results are ordered by <see cref="PhysiologicalMeasurement.DateOfMeasure"/> descending.
        /// </remarks>
        /// <param name="id">id of the patient to get most recent measures from</param>
        /// <param name="count">Number of result to get at most</param>
        /// <returns>Array of <see cref="BloodPressureInfo"/></returns>
        [HttpGet("{id:int}/[action]")]
        [HttpHead("{id:int}/[action]")]
        [ProducesResponseType(typeof(IEnumerable<BloodPressureInfo>), 200)]
        public async Task<IEnumerable<BloodPressureInfo>> MostRecentBloodPressures([Range(1, int.MaxValue)] int id, [Range(1, int.MaxValue)] int? count)
            => await MostRecentMeasureAsync<BloodPressure, BloodPressureInfo>(new GetMostRecentPhysiologicalMeasuresInfo { PatientId = id, Count = count });

        /// <summary>
        /// Get the last <see cref="TemperatureInfo"/> measures.
        /// </summary>
        /// <remarks>
        /// Results are ordered by <see cref="PhysiologicalMeasurement.DateOfMeasure"/> descending.
        /// </remarks>
        /// <param name="id">id of the patient to get most recent measures from</param>
        /// <param name="count">Number of result to get at most</param>
        /// <returns>Array of <see cref="TemperatureInfo"/></returns>
        [HttpGet("{id:int}/[action]")]
        [HttpHead("{id:int}/[action]")]
        public async Task<IEnumerable<TemperatureInfo>> MostRecentTemperatures([Range(1, int.MaxValue)] int id, [Range(1, int.MaxValue)]int? count)
            => await MostRecentMeasureAsync<Temperature, TemperatureInfo>(new GetMostRecentPhysiologicalMeasuresInfo { PatientId = id, Count = count });



        /// <summary>
        /// Search patients resource based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <remarks>
        /// All criteria are combined as a AND.
        /// 
        /// Advanded search :
        /// Several operators that can be used to make an advanced search :
        /// '*' : match zero or more characters in a string property.
        /// 
        ///     // GET api/Patients/Search?Firstname=Bruce
        ///     will match all resources which have exactly 'Bruce' in the Firstname property
        ///     
        ///     // GET api/Patients/Search?Firstname=B*e
        ///     will match match all resources which starts with 'B' and ends with 'e'.
        /// 
        /// '?' : match exactly one charcter in a string property.
        /// 
        /// '!' : negate a criteria
        /// 
        ///     // GET api/Patients/Search?Firstname=!Bruce
        ///     will match all resources where Firstname is not "Bruce"
        ///     
        /// </remarks>
        /// <response code="200">Array of resources that matches <paramref name="search"/> criteria.</response>
        /// <response code="400">one the search criteria is not valid</response>
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(IEnumerable<PatientInfo>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), 400)]
        public async Task<IActionResult> Search([FromQuery] SearchPatientInfo search)
        {
            
            
            IList<IDataFilter> filters = new List<IDataFilter>();
            if (!string.IsNullOrWhiteSpace(search.Firstname))
            {
                filters.Add($"{nameof(PatientInfo.Firstname)}={search.Firstname}".ToFilter<PatientInfo>());
            }

            if (!string.IsNullOrWhiteSpace(search.Lastname))
            {
                filters.Add($"{nameof(PatientInfo.Lastname)}={search.Lastname}".ToFilter<PatientInfo>());
            }

            SearchQueryInfo<PatientInfo> searchQueryInfo = new SearchQueryInfo<PatientInfo>
            {
                Page = search.Page,
                PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize),
                Filter = filters.Count() == 1
                    ? filters.Single()
                    : new DataCompositeFilter { Logic = And, Filters = filters },
                Sorts = (search.Sort ?? $"-{nameof(PatientInfo.UpdatedDate)}").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x =>
                        {
                            x = x.Trim();
                            Sort sort;
                            if (x.StartsWith("-"))
                            {
                                x = x.Substring(1);
                                sort = new Sort { Direction = Data.SortDirection.Descending, Expression = x.ToLambda<PatientInfo>() };
                            }
                            else
                            {
                                sort = new Sort { Direction = Data.SortDirection.Ascending, Expression = x.ToLambda<PatientInfo>() };
                            }

                            return sort;
                        })
            };

            

            IPagedResult<PatientInfo> pageOfResult = await _iHandleSearchQuery.Search<Patient, PatientInfo>(new SearchQuery<PatientInfo>(searchQueryInfo));

            search.PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize);
            int count = pageOfResult.Entries.Count();
            bool hasPreviousPage = count > 0 && search.Page > 1;

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            string firstPageUrl = urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, Page = 1, search.PageSize, search.Sort });
            string previousPageUrl = hasPreviousPage
                    ? urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, Page = search.Page - 1, search.PageSize, search.Sort })
                    : null;

            string nextPageUrl = search.Page < pageOfResult.PageCount
                    ? urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, Page = search.Page + 1, search.PageSize, search.Sort } )
                    : null;
            string lastPageUrl = pageOfResult.PageCount > 1
                    ? urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, Page = pageOfResult.PageCount, search.PageSize, search.Sort } )
                    : null;

            IGenericPagedGetResponse<PatientInfo> reponse = new GenericPagedGetResponse<PatientInfo>(
                pageOfResult.Entries,
                first: firstPageUrl,
                previous: previousPageUrl,
                next: nextPageUrl,
                last: lastPageUrl,
                count : pageOfResult.Total);

            return new OkObjectResult(reponse);

        }


        /// <summary>
        /// Partially update a patient resource.
        /// </summary>
        /// <remarks>
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        ///
        ///     // PATCH api/Patients/1
        ///     
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/MainDoctorId",
        ///             "from": "string",
        ///             "value": 1
        ///       }
        ///     ]
        /// 
        /// The set of changes to apply will be applied atomically. 
        /// 
        /// </remarks>
        /// <param name="id">id of the resource to update</param>
        /// <param name="changes">set of changes to apply to the resource</param>
        /// <response code="200">The resource was successfully patched </response>
        /// <response code="400">Changes are not valid</response>
        /// <response code="404">Resource to "PATCH" not found</response>
        [HttpPatch("{id:int}")]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<PatientInfo> changes)
        {
            PatchInfo<int, Patient> data = new PatchInfo<int, Patient>
            {
                Id = id,
                PatchDocument = _mapper.Map<JsonPatchDocument<Patient>>(changes)
            };
            await _iRunPatchPatientCommmand.RunAsync(new PatchCommand<int, Patient>(data));


            return new OkResult();
        }

        /// <summary>
        /// Gets one patient's <see cref="BodyWeightInfo"/>.
        /// </summary>
        /// <param name="id">patient id</param>
        /// <param name="bodyWeightId">id of the <see cref="BodyWeightInfo"/> resource to get</param>
        /// <response code="200">the resource was found</response>
        /// <response code="400">either <paramref name="id"/> or <paramref name="bodyWeightId"/> is negative or zero</response>
        /// <response code="404"><paramref name="id"/> does not identify a <see cref="PatientInfo"/> resource or <paramref name="bodyWeightId"/></response> 
        [HttpGet("{id:int}/[action]/{bodyWeightId:int}")]
        [HttpHead("{id:int}/[action]/{bodyWeightId:int}")]
        [ProducesResponseType(typeof(BodyWeightInfo), 200)]
        public async Task<IActionResult> BodyWeights([Range(1, int.MaxValue)] int id, [Range(1, int.MaxValue)] int bodyWeightId)
        {
            BodyWeightInfo output = await _physiologicalMeasureService.GetOneMeasureAsync<BodyWeight, BodyWeightInfo>(new WantOnePhysiologicalMeasureQuery<BodyWeightInfo>(id, bodyWeightId));
            IActionResult actionResult = null;

            if (output != null)
            {
                IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                actionResult = new OkObjectResult(new BrowsableResource<BodyWeightInfo>
                {
                    Resource = output,
                    Links = new[] {
                        new Link
                        {
                            Href = urlHelper.Action(nameof(BodyWeights), EndpointName, new { id = output.PatientId, bodyWeightId = output.Id }),
                            Rel = "self"
                        }
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
        /// Delete the specified blood pressure resource
        /// </summary>
        /// <param name="input"></param>
        /// <response code="200">the operation succeed</response>
        /// <response code="400">if the operation is not allowed</response>
        [HttpDelete("{id:int}/[action]/{measureId:int}")]
        public async Task<IActionResult> BloodPressures([FromQuery] DeletePhysiologicalMeasureInfo input)
        {
            await DeleteOneMeasureAsync<BloodPressure>(input);
            return new OkResult();
        }

        /// <summary>
        /// Delete the specified blood pressure resource
        /// </summary>
        /// <param name="input"></param>
        /// <response code="200">if the operation succeed</response>
        /// <response code="400">if the operation is not allowed</response>
        [HttpDelete("{id:int}/[action]/{measureId:int}")]
        public async Task<IActionResult> Temperatures([FromQuery] DeletePhysiologicalMeasureInfo input)
        {
            await DeleteOneMeasureAsync<Temperature>(input);
            return new OkResult();
        }

        /// <summary>
        /// Delete the specified body weight resource
        /// </summary>
        /// <param name="input"></param>
        /// <response code="200">if the operation succeed</response>
        /// <response code="400">if the operation is not allowed</response>
        [HttpDelete("{id:int}/[action]/{measureId:int}")]
        public async Task<IActionResult> BodyWeights([FromQuery] DeletePhysiologicalMeasureInfo input)
        {
            await DeleteOneMeasureAsync<BodyWeight>(input);
            return new OkResult();
        }

        /// <summary>
        /// Gets mot recents <see cref="PhysiologicalMeasurement"/>
        /// </summary>
        /// <typeparam name="TPhysiologicalMeasure"></typeparam>
        /// <typeparam name="TPhysiologicalMeasureInfo"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<IEnumerable<TPhysiologicalMeasureInfo>> MostRecentMeasureAsync<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(GetMostRecentPhysiologicalMeasuresInfo query)
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            where TPhysiologicalMeasureInfo : PhysiologicalMeasurementInfo
            => await _physiologicalMeasureService.GetMostRecentMeasuresAsync<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(new WantMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasureInfo>(query));


        /// <summary>
        /// Gets mot recents <see cref="PhysiologicalMeasurement"/>
        /// </summary>
        /// <typeparam name="TPhysiologicalMeasure">Type of measure to delete</typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private async Task DeleteOneMeasureAsync<TPhysiologicalMeasure>(DeletePhysiologicalMeasureInfo input)
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            => await _physiologicalMeasureService.DeleteOnePhysiologicalMeasureAsync<TPhysiologicalMeasure>(new DeleteOnePhysiologicalMeasureCommand(input));

        /// <summary>
        /// Gets one of the patient's prescription
        /// </summary>
        /// <param name="id">Id of the patient</param>
        /// <param name="prescriptionId">Identifier of the prescription to get</param>
        /// <returns></returns>
        /// <response code="200">if the prescription was found</response>
        /// <response code="404">no prescription with the <paramref name="prescriptionId"/> found</response>
        /// <response code="404">no patient with the <paramref name="id"/> found</response>
        [HttpGet("{id:int}/[action]/{prescriptionId:int}")]
        [HttpHead("{id:int}/[action]/{prescriptionId:int}")]
        [ProducesResponseType(typeof(PrescriptionHeaderInfo), 200)]
        public async Task<IActionResult> Prescriptions(int id, int prescriptionId)
        {
            PrescriptionHeaderInfo output = await _prescriptionService.GetOnePrescriptionByPatientIdAsync(id, prescriptionId);
            IActionResult actionResult;
            if (output == null)
            {
                actionResult = new NotFoundResult();
            }
            else
            {
                IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                IBrowsableResource<PrescriptionHeaderInfo> browsableResource = new BrowsableResource<PrescriptionHeaderInfo>
                {
                    Links = new[] {
                        new Link
                        {
                            Rel = "self",
                            Href = urlHelper.Action(nameof(Prescriptions), EndpointName, new { id = output.PatientId, prescriptionId = output.Id})
                        },
                        new Link
                        {
                            Rel = nameof(Prescription.Items),
                            Href = urlHelper.Action(nameof(PrescriptionsController.Details), PrescriptionsController.EndpointName , new { output.Id })
                        }
                    },
                    Resource = output
                };

                actionResult = new OkObjectResult(browsableResource);
            }

            return actionResult;
        }


        /// <summary>
        /// Create a new prescription for a patient
        /// </summary>
        /// <param name="id">id of the patient</param>
        /// <param name="newPrescription">prescription details</param>
        /// <returns></returns>
        /// <response code="201">The header of the prescription.</response>
        /// <response code="400">if <paramref name="newPrescription"/> contains invalid data.</response>
        [HttpPost("{id:int}/[action]")]
        [ProducesResponseType(typeof(PrescriptionHeaderInfo), 201)]
        public async Task<IActionResult> Prescriptions(int id, [FromBody] CreatePrescriptionInfo newPrescription)
        {

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            
            PrescriptionHeaderInfo createdPrescription = await _prescriptionService.CreatePrescriptionForPatientAsync(id, newPrescription);
            IBrowsableResource<PrescriptionHeaderInfo> browsableResource = new BrowsableResource<PrescriptionHeaderInfo>
            {
                Resource = createdPrescription,
                Links = new[]
                {
                    new Link {
                        Href = urlHelper.Action(nameof(PrescriptionsController.Details), PrescriptionsController.EndpointName, new {id = createdPrescription.Id}),
                        Method = "GET",
                        Rel = nameof(Prescription.Items),
                        Title = "Content"
                    }
                }
            };
            return new CreatedAtActionResult(nameof(Prescriptions), EndpointName, new { id = createdPrescription.PatientId, prescriptionId = createdPrescription.Id }, browsableResource);
        }


        /// <summary>
        /// Gets the most recent prescriptions
        /// </summary>
        /// <remarks>
        ///     Only the metadata of the prescriptions are retireved from here. To get the full content of the prescriptions
        ///     You should call :
        ///     
        ///     // api/Prescriptions/{id}/Items
        ///     
        ///     where {id} is the id of the prescription
        ///     
        /// </remarks>
        /// <param name="id">id of the patient to get the most </param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <reponse code="200">List of the most recent prescriptions' metadata</reponse>
        /// <reponse code="404">List of prescriptions' metadata</reponse>
        [HttpGet("{id:int}/[action]")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionHeaderInfo>), 200)]
        public async Task<IEnumerable<PrescriptionHeaderInfo>> MostRecentPrescriptions(int id, int? count)
        {
            GetMostRecentPrescriptionsInfo input = new GetMostRecentPrescriptionsInfo { PatientId = id, Count = count };
            IEnumerable<PrescriptionHeaderInfo> prescriptions = await _prescriptionService.GetMostRecentPrescriptionsAsync(new WantMostRecentPrescriptionsQuery(input));

            return prescriptions;
        }


        /// <summary>
        /// Adds a new <see cref="BodyWeightInfo"/> for the patient with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">id of the patient the measure will be created for</param>
        /// <param name="newBodyWeight">measure to add</param>
        /// <response code="201">the resource created successfully</response>
        /// <response code="400"><paramref name="newBodyWeight"/> is not valid or <paramref name="id"/> is negative or zero</response>
        [HttpPost("{id:int}/[action]")]
        [ProducesResponseType(typeof(BodyWeightInfo), 200)]
        public async Task<IActionResult> BodyWeights(int id, [FromBody] CreateBodyWeightInfo newBodyWeight)
        {
            BodyWeight newMeasure = new BodyWeight
            {
                PatientId = id,
                DateOfMeasure = newBodyWeight.DateOfMeasure,
                Value = newBodyWeight.Value
            };
            BodyWeightInfo output = await _physiologicalMeasureService.AddNewMeasureAsync<BodyWeight, BodyWeightInfo>(new AddNewPhysiologicalMeasureCommand<BodyWeight, BodyWeightInfo>(newMeasure));
            return new CreatedAtActionResult(nameof(BodyWeights), EndpointName, new { id = output.PatientId, bodyWeightId = output.Id }, output);
        }

        /// <summary>
        /// Generates additional id for the patient resource
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="urlHelper"></param>
        /// <returns></returns>
        protected override IEnumerable<Link> BuildAdditionalLinksForResource(PatientInfo resource, IUrlHelper urlHelper)
            => resource.MainDoctorId.HasValue
                ? new[]
                {
                    new Link {
                        Rel = "main-doctor-id",
                        Href = urlHelper.Action(nameof(DoctorsController.Get), DoctorsController.EndpointName, new { id = resource.MainDoctorId })
                    }
                }
                : Enumerable.Empty<Link>();


        /// <summary>
        /// Gets all patient's documents metadata
        /// </summary>
        /// <remarks>
        /// This method gets all documents' metadata that are related to the patient <paramref name="id"/>.
        /// </remarks>
        /// <param name="id">id of the patient to get documents from</param>
        /// <param name="page">Index of the page of result set (the first page is 1).</param>
        /// <param name="pageSize">Size of a page of results.</param>
        /// <returns></returns>
        /// <response code="200">The documents' metadata.</response>
        /// <response code="404">if no patient found.</response>
        [HttpGet("{id}/[action]")]
        [ProducesResponseType(typeof(IEnumerable<DocumentMetadataInfo>), 200)]
        public async Task<IActionResult> Documents(int id, int page, int pageSize)
        {
            GenericGetQuery query = new GenericGetQuery
            {
                Page = page,
                PageSize = Math.Min(ApiOptions.Value.MaxPageSize, pageSize)
            };

            IPagedResult<DocumentMetadataInfo> result = await _iHandleGetDocumentByPatientIdQuery.HandleAsync(new WantDocumentsByPatientIdQuery(id, query));

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && query.Page > 1;

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            string firstPageUrl = urlHelper.Action(nameof(Documents), ControllerName, new { PageSize = query.PageSize, Page = 1, id });
            string previousPageUrl = hasPreviousPage
                    ? urlHelper.Action(nameof(Documents), ControllerName, new { PageSize = query.PageSize, Page = query.Page - 1, id, })
                    : null;

            string nextPageUrl = query.Page < result.PageCount
                    ? urlHelper.Action(nameof(Documents), ControllerName, new { PageSize = query.PageSize, Page = query.Page + 1, id })
                    : null;
            string lastPageUrl = result.PageCount > 0
                    ? urlHelper.Action(nameof(Documents), ControllerName, new { PageSize = query.PageSize, Page = result.PageCount, id })
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
    }

}
