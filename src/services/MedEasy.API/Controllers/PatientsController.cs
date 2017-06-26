using AutoMapper;
using MedEasy.Commands;
using MedEasy.Commands.Patient;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.DTO;
using MedEasy.Objects;
using MedEasy.Queries.Patient;
using MedEasy.Queries.Prescriptions;
using MedEasy.Queries.Search;
using MedEasy.RestObjects;
using MedEasy.Services;
using MedEasy.Validators;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MedEasy.Data.DataFilterLogic;
using MedEasy.Handlers.Core.Patient.Commands;
using MedEasy.Handlers.Core.Search.Queries;
using MedEasy.Handlers.Core.Patient.Queries;
using Microsoft.AspNetCore.Http;
using System.IO;
using MedEasy.DTO.Search;
using System.Threading;
using Optional;
using MedEasy.Handlers.Core.Exceptions;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="PatientInfo"/> resources
    /// </summary>
    [Route("api/[controller]")]
    public class PatientsController : RestCRUDControllerBase<Guid, Patient, PatientInfo, IWantOnePatientInfoByIdQuery, IWantPageOfPatientInfoQuery, Guid, CreatePatientInfo, ICreatePatientCommand, IRunCreatePatientCommand>
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>²
        public static string EndpointName => nameof(PatientsController).Replace("Controller", string.Empty);


        /// <summary>
        /// Name of the controller without the "Controller" suffix 
        /// </summary>
        protected override string ControllerName => EndpointName;

        private readonly IRunCreatePatientCommand _iRunCreatePatientCommand;
        private readonly IRunDeletePatientByIdCommand _iRunDeletePatientByIdCommand;

        private readonly IPhysiologicalMeasureService _physiologicalMeasureService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly IRunPatchPatientCommand _iRunPatchPatientCommmand;
        private readonly IMapper _mapper;
        private readonly IHandleSearchQuery _iHandleSearchQuery;
        private readonly IHandleGetDocumentsByPatientIdQuery _iHandleGetDocumentByPatientIdQuery;

        private readonly IRunCreateDocumentForPatientCommand _iRunCreateDocumentForPatientCommand;

        private readonly IHandleGetOneDocumentInfoByPatientIdAndDocumentId _iHandleGetOneDocumentInfoByPatientIdAndDocumentId;

        /// <summary>
        /// Builds a new <see cref="PatientsController"/> instance
        /// </summary>
        /// <param name="getByIdQueryHandler">Handler of GET one resource</param>
        /// <param name="apiOptions">Options of the API</param>
        /// <param name="getManyPatientQueryHandler">Handler of GET many <see cref="PatientInfo"/> resources</param>
        /// <param name="iRunCreatePatientCommand">Runner of CREATE resource command</param>
        /// <param name="iRunDeletePatientByIdCommand">Runner of DELETE resource command</param>
        /// <param name="logger">logger</param>
        /// <param name="iHandleSearchQuery">handler for <see cref="SearchQueryInfo{T}"/></param>
        /// <param name="urlHelper">Factory used to build <see cref="IUrlHelper"/> instances.</param>
        /// <param name="physiologicalMeasureService">Service that deals with everything that's related to <see cref="PhysiologicalMeasurementInfo"/> resources</param>
        /// <param name="prescriptionService">Service that deals with everything that's related to <see cref="PrescriptionInfo"/> resources</param>
        /// <param name="iRunPatchPatientCommmand">Runner for changing main doctor ID command.</param>
        /// <param name="iHandleGetDocumentByPatientIdQuery">Handler for retrieving patient's <see cref="DocumentMetadataInfo"/>s.</param>
        /// <param name="mapper">Mapper to convert one type to an other.</param>
        /// <param name="iRunCreateDocumentForPatientCommand">Runner for CREATE document resource commands.</param>
        /// <param name="iHandleGetOneDocumentInfoByPatientIdAndDocumentId">Runner for CREATE document resource commands.</param>
        public PatientsController(ILogger<PatientsController> logger, IUrlHelper urlHelper,
            IOptionsSnapshot<MedEasyApiOptions> apiOptions,
            IMapper mapper,
            IHandleSearchQuery iHandleSearchQuery,
            IHandleGetOnePatientInfoByIdQuery getByIdQueryHandler,
            IHandleGetPageOfPatientInfosQuery getManyPatientQueryHandler,
            IRunCreatePatientCommand iRunCreatePatientCommand,
            IRunDeletePatientByIdCommand iRunDeletePatientByIdCommand,
            IPhysiologicalMeasureService physiologicalMeasureService,
            IPrescriptionService prescriptionService,
            IHandleGetDocumentsByPatientIdQuery iHandleGetDocumentByPatientIdQuery,
            IRunPatchPatientCommand iRunPatchPatientCommmand,
            IRunCreateDocumentForPatientCommand iRunCreateDocumentForPatientCommand,
            IHandleGetOneDocumentInfoByPatientIdAndDocumentId iHandleGetOneDocumentInfoByPatientIdAndDocumentId) :
            base(logger, apiOptions, getByIdQueryHandler, getManyPatientQueryHandler, iRunCreatePatientCommand, urlHelper)
        {
            _iRunCreatePatientCommand = iRunCreatePatientCommand;
            _iRunDeletePatientByIdCommand = iRunDeletePatientByIdCommand;
            _physiologicalMeasureService = physiologicalMeasureService;
            _prescriptionService = prescriptionService;
            _iRunPatchPatientCommmand = iRunPatchPatientCommmand;
            _iHandleSearchQuery = iHandleSearchQuery;
            _iHandleGetDocumentByPatientIdQuery = iHandleGetDocumentByPatientIdQuery;
            _iRunCreateDocumentForPatientCommand = iRunCreateDocumentForPatientCommand;
            _iHandleGetOneDocumentInfoByPatientIdAndDocumentId = iHandleGetOneDocumentInfoByPatientIdAndDocumentId;
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

            PaginationConfiguration pageConfig = new PaginationConfiguration
            {
                Page = page,
                PageSize = pageSize
            };
            IPagedResult<PatientInfo> result = await GetAll(pageConfig);

            int count = result.Entries.Count();
            bool hasPreviousPage = count > 0 && pageConfig.Page > 1;

            string firstPageUrl = UrlHelper.Action(nameof(Get), ControllerName, new { PageSize = pageConfig.PageSize, Page = 1 });
            string previousPageUrl = hasPreviousPage
                    ? UrlHelper.Action(nameof(Get), ControllerName, new { PageSize = pageConfig.PageSize, Page = pageConfig.Page - 1 })
                    : null;

            string nextPageUrl = pageConfig.Page < result.PageCount
                    ? UrlHelper.Action(nameof(Get), ControllerName, new { PageSize = pageConfig.PageSize, Page = pageConfig.Page + 1 })
                    : null;
            string lastPageUrl = result.PageCount > 0
                    ? UrlHelper.Action(nameof(Get), ControllerName, new { PageSize = pageConfig.PageSize, Page = result.PageCount })
                    : null;


            await result.Entries.ForEachAsync((x) => Task.FromResult(x.Meta = new Link { Href = UrlHelper.Action(nameof(Get), new { x.Id }) }));


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
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">The patient resource</response>
        /// <response code="404">Resource not found</response>
        [HttpHead("{id}")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BrowsableResource<PatientInfo>), 200)]
        public async override Task<IActionResult> Get(Guid id, CancellationToken cancellationToken = default(CancellationToken))
            => await base.Get(id, cancellationToken);

        /// <summary>
        /// Creates a new <see cref="PatientInfo"/> resource.
        /// </summary>
        /// <param name="newPatient">data used to create the resource</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="201">the resource was created successfully</response>
        /// <response code="400"><paramref name="newPatient"/> is not valid</response>
        [HttpPost]
        [ProducesResponseType(typeof(BrowsableResource<PatientInfo>), 201)]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Post([FromBody] CreatePatientInfo newPatient, CancellationToken cancellationToken = default(CancellationToken))
        {
            Option<PatientInfo, CommandException> resource = await _iRunCreatePatientCommand.RunAsync(new CreatePatientCommand(newPatient), cancellationToken);

            return resource.Match(
                some: patient =>
               {
                   IBrowsableResource<PatientInfo> browsableResource = new BrowsableResource<PatientInfo>
                   {
                       Resource = patient,
                       Links = BuildAdditionalLinksForResource(patient)
                   };
                   return new CreatedAtActionResult(nameof(Get), EndpointName, new { patient.Id }, browsableResource);

               },
                none: exception =>
                {
                    IActionResult result;
                    switch (exception)
                    {
                        case CommandEntityNotFoundException cenf:
                            result = new BadRequestObjectResult(cenf.Message);
                            break;
                        default:
                            result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                            break;
                    }

                    return result;

                });
        }


        /// <summary>
        /// Updates the specified resource
        /// </summary>
        /// <remarks>
        /// The resource's current value will completely be replace
        /// </remarks>
        /// <param name="id">identifier of the resource to update</param>
        /// <param name="info">new values to set</param>
        /// <returns></returns>
        /// <response code="200">the operation succeed</response>
        /// <response code="400">Submitted values contains an error</response>
        /// <response code="404">Resource not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PatientInfo), 200)]
        public Task<IActionResult> Put(Guid id, [FromBody] CreatePatientInfo info)
        {
            throw new NotImplementedException();
        }

        // DELETE api/patients/5

        /// <summary>
        /// Delete the <see cref="PatientInfo"/> by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="200">if the operation succeed</response>
        /// <response code="400">if <paramref name="id"/> is empty.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        {
            IActionResult actionResult;
            if (id == Guid.Empty)
            {
                actionResult = new BadRequestResult();
            }
            else
            {
                await _iRunDeletePatientByIdCommand.RunAsync(new DeletePatientByIdCommand(id), cancellationToken);
                actionResult = new NoContentResult();
            }

            return actionResult;
        }


        /// <summary>
        /// Create a new <see cref="TemperatureInfo"/> resource.
        /// </summary>
        /// <param name="id">id of the patient the new measure will be attached to</param>
        /// <param name="newTemperature">input to create the new resource</param>
        /// <see cref="IPhysiologicalMeasureService.AddNewMeasureAsync{TPhysiologicalMeasure, TPhysiologicalMeasureInfo}(ICommand{Guid, CreatePhysiologicalMeasureInfo{TPhysiologicalMeasure}}, CancellationToken)"/>
        /// <response code="201">if the creation succeed</response>
        /// <response code="400"><paramref name="newTemperature"/> is not valid or <paramref name="id"/> is negoative or zero</response>.
        /// <response code="40'">patient not found.</response>.
        [HttpPost("{id}/[action]")]
        [ProducesResponseType(typeof(TemperatureInfo), 201)]
        [ProducesResponseType(typeof(ModelStateDictionary), 400)]
        public async Task<IActionResult> Temperatures(Guid id, [FromBody] CreateTemperatureInfo newTemperature)
        {
            CreatePhysiologicalMeasureInfo<Temperature> input = new CreatePhysiologicalMeasureInfo<Temperature>
            {
                PatientId = id,
                Measure = new Temperature
                {
                    DateOfMeasure = newTemperature.DateOfMeasure,
                    Value = newTemperature.Value
                }
            };

            TemperatureInfo output = await _physiologicalMeasureService
                .AddNewMeasureAsync<Temperature, TemperatureInfo>(new AddNewPhysiologicalMeasureCommand<Temperature, TemperatureInfo>(input))
                .ConfigureAwait(false);

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
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="201">the resource creation succeed</response>
        /// <response code="400"><paramref name="newBloodPressure"/> is not valid or <paramref name="id"/> is negative or zero</response>
        [HttpPost("{id}/[action]")]
        [ProducesResponseType(typeof(BloodPressureInfo), 200)]
        public async Task<IActionResult> BloodPressures(Guid id, [FromBody] CreateBloodPressureInfo newBloodPressure, CancellationToken cancellationToken = default(CancellationToken))
        {
            CreatePhysiologicalMeasureInfo<BloodPressure> info = new CreatePhysiologicalMeasureInfo<BloodPressure>
            {
                PatientId = id,
                Measure = new BloodPressure
                {
                    DateOfMeasure = newBloodPressure.DateOfMeasure,
                    SystolicPressure = newBloodPressure.SystolicPressure,
                    DiastolicPressure = newBloodPressure.DiastolicPressure
                }
            };
            BloodPressureInfo output = await _physiologicalMeasureService.AddNewMeasureAsync<BloodPressure, BloodPressureInfo>(new AddNewPhysiologicalMeasureCommand<BloodPressure, BloodPressureInfo>(info))
                .ConfigureAwait(false);

            return new CreatedAtActionResult(nameof(BloodPressures), EndpointName, new { id = output.PatientId, bloodPressureId = output.Id }, output);
        }

        /// <summary>
        /// Gets one mesure of temperature for the specified patient
        /// </summary>
        /// <param name="id">Id of the <see cref="PatientInfo"/>.</param>
        /// <param name="temperatureId">id of the <see cref="TemperatureInfo"/> to get</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        [HttpGet("{id}/[action]/{temperatureId}")]
        [HttpHead("{id}/[action]/{temperatureId}")]
        [ProducesResponseType(typeof(TemperatureInfo), 200)]
        [ProducesResponseType(typeof(ModelStateDictionary), 400)]
        public async Task<IActionResult> Temperatures(Guid id, Guid temperatureId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Option<TemperatureInfo> output = await _physiologicalMeasureService.GetOneMeasureAsync<Temperature, TemperatureInfo>(new WantOnePhysiologicalMeasureQuery<TemperatureInfo>(id, temperatureId), cancellationToken);

            return output.Match<IActionResult>(
                some: x =>
                {
                    x.Meta = new Link
                    {
                        Href = UrlHelper.Action(nameof(Temperatures), EndpointName, new { id = x.PatientId, temperatureId = x.Id }),
                        Relation = "self",
                        Method = "GET"
                    };

                    return new OkObjectResult(new BrowsableResource<TemperatureInfo>
                    {
                        Resource = x,
                        Links = new[] {
                        new Link { Href = UrlHelper.Action(nameof(Temperatures), EndpointName, new { x.Id }), Relation = "remove", Method = "DELETE" },
                        new Link { Href = UrlHelper.Action(nameof(Temperatures), EndpointName, new { x.Id }), Relation = "direct-link", Method = "GET" }
                    }
                    });
                },

                none: () => new NotFoundResult());
        }

        /// <summary>
        /// Gets one mesure of temperature for the specified patient
        /// </summary>
        /// <param name="id">Id of the <see cref="PatientInfo"/>.</param>
        /// <param name="bloodPressureId">id of the <see cref="BloodPressureInfo"/> to get</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        [HttpGet("{id}/[action]/{bloodPressureId}")]
        [HttpHead("{id}/[action]/{bloodPressureId}")]
        [ProducesResponseType(typeof(BloodPressureInfo), 200)]
        public async Task<IActionResult> BloodPressures(Guid id, Guid bloodPressureId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Option<BloodPressureInfo> output = await _physiologicalMeasureService
                .GetOneMeasureAsync<BloodPressure, BloodPressureInfo>(new WantOnePhysiologicalMeasureQuery<BloodPressureInfo>(id, bloodPressureId), cancellationToken);

            return output.Match<IActionResult>(

                some: x =>
                {
                    x.Meta = new Link
                    {
                        Href = UrlHelper.Action(nameof(BloodPressures), EndpointName, new { id = x.PatientId, temperatureId = x.Id }),
                        Relation = "self"
                    };
                    return new OkObjectResult(new BrowsableResource<BloodPressureInfo>
                    {
                        Resource = x
                    });
                },
            none: () => new NotFoundResult());
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
        [HttpGet("{id}/[action]")]
        [HttpHead("{id}/[action]")]
        [ProducesResponseType(typeof(IEnumerable<BloodPressureInfo>), 200)]
        public async Task<IEnumerable<BloodPressureInfo>> MostRecentBloodPressures(Guid id, [FromQuery] int? count)
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
        [HttpGet("{id}/[action]")]
        [HttpHead("{id}/[action]")]
        public async Task<IEnumerable<TemperatureInfo>> MostRecentTemperatures(Guid id, [FromQuery]int? count)
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

            if (search.BirthDate.HasValue)
            {

                filters.Add(new DataFilter { Field = nameof(PatientInfo.BirthDate), Operator = DataFilterOperator.EqualTo, Value = search.BirthDate.Value });
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

            string firstPageUrl = UrlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, search.BirthDate, Page = 1, search.PageSize, search.Sort });
            string previousPageUrl = hasPreviousPage
                    ? UrlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, search.BirthDate, Page = search.Page - 1, search.PageSize, search.Sort })
                    : null;

            string nextPageUrl = search.Page < pageOfResult.PageCount
                    ? UrlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, search.BirthDate, Page = search.Page + 1, search.PageSize, search.Sort })
                    : null;
            string lastPageUrl = pageOfResult.PageCount > 1
                    ? UrlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, search.BirthDate, Page = pageOfResult.PageCount, search.PageSize, search.Sort })
                    : null;

            await pageOfResult.Entries.ForEachAsync((x) => Task.FromResult(x.Meta = new Link { Href = UrlHelper.Action(nameof(Get), new { x.Id }) }));


            IGenericPagedGetResponse<PatientInfo> reponse = new GenericPagedGetResponse<PatientInfo>(
                pageOfResult.Entries,
                first: firstPageUrl,
                previous: previousPageUrl,
                next: nextPageUrl,
                last: lastPageUrl,
                count: pageOfResult.Total);

            return new OkObjectResult(reponse);

        }


        /// <summary>
        /// Partially update a patient resource.
        /// </summary>
        /// <remarks>
        /// Use the <paramref name="changes"/> to declare all modifications to apply to the resource.
        /// Only the declared modifications will be applied to the resource.
        ///
        ///     // PATCH api/Patients/3594c436-8595-444d-9e6b-2686c4904725
        ///     
        ///     [
        ///         {
        ///             "op": "update",
        ///             "path": "/MainDoctorId",
        ///             "from": "string",
        ///             "value": "e1aa24f4-69a8-4d3a-aca9-ec15c6910dc9"
        ///       }
        ///     ]
        /// 
        /// The set of changes to apply will be applied atomically. 
        /// 
        /// </remarks>
        /// <param name="id">id of the resource to update.</param>
        /// <param name="changes">set of changes to apply to the resource.</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">The resource was successfully patched.</response>
        /// <response code="400">Changes are not valid for the selected resource.</response>
        /// <response code="404">Resource to "PATCH" not found</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IEnumerable<ErrorInfo>), 400)]
        public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<PatientInfo> changes, CancellationToken cancellationToken = default(CancellationToken))
        {
            PatchInfo<Guid, Patient> data = new PatchInfo<Guid, Patient>
            {
                Id = id,
                PatchDocument = _mapper.Map<JsonPatchDocument<Patient>>(changes)
            };
            await _iRunPatchPatientCommmand.RunAsync(new PatchCommand<Guid, Patient>(data), cancellationToken);

            return new NoContentResult();
        }

        /// <summary>
        /// Gets one patient's <see cref="BodyWeightInfo"/>.
        /// </summary>
        /// <param name="id">patient id</param>
        /// <param name="bodyWeightId">id of the <see cref="BodyWeightInfo"/> resource to get</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">the resource was found</response>
        /// <response code="400">either <paramref name="id"/> or <paramref name="bodyWeightId"/> is negative or zero</response>
        /// <response code="404"><paramref name="id"/> does not identify a <see cref="PatientInfo"/> resource or <paramref name="bodyWeightId"/></response> 
        [HttpGet("{id}/[action]/{bodyWeightId}")]
        [HttpHead("{id}/[action]/{bodyWeightId}")]
        [ProducesResponseType(typeof(BodyWeightInfo), 200)]
        public async Task<IActionResult> BodyWeights(Guid id, Guid bodyWeightId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Option<BodyWeightInfo> output = await _physiologicalMeasureService.GetOneMeasureAsync<BodyWeight, BodyWeightInfo>(new WantOnePhysiologicalMeasureQuery<BodyWeightInfo>(id, bodyWeightId));

            return output.Match<IActionResult>(
                some: x =>
                   {
                       x.Meta = new Link
                       {
                           Href = UrlHelper.Action(nameof(BodyWeights), EndpointName, new { id = x.PatientId, bodyWeightId = x.Id }),
                           Relation = "self"
                       };
                       return new OkObjectResult(new BrowsableResource<BodyWeightInfo>
                       {
                           Resource = x
                       });
                   },
                none: () => new NotFoundResult());
        }


        /// <summary>
        /// Delete the specified blood pressure resource
        /// </summary>
        /// <param name="input"></param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <response code="200">the operation succeed</response>
        /// <response code="400">if the operation is not allowed</response>
        [HttpDelete("{id}/[action]/{measureId}")]
        public async Task<IActionResult> BloodPressures(DeletePhysiologicalMeasureInfo input, CancellationToken cancellationToken = default(CancellationToken))
        {
            await DeleteOneMeasureAsync<BloodPressure>(input, cancellationToken);
            return new NoContentResult();
        }

        /// <summary>
        /// Delete the specified blood pressure resource
        /// </summary>
        /// <param name="input"></param>
        /// <response code="200">if the operation succeed</response>
        /// <response code="400">if the operation is not allowed</response>
        [HttpDelete("{id}/[action]/{measureId}")]
        public async Task<IActionResult> Temperatures(DeletePhysiologicalMeasureInfo input)
        {
            await DeleteOneMeasureAsync<Temperature>(input);
            return new NoContentResult();
        }

        /// <summary>
        /// Delete the specified body weight resource
        /// </summary>
        /// <param name="input"></param>
        /// <response code="200">if the operation succeed</response>
        /// <response code="400">if the operation is not allowed</response>
        [HttpDelete("{id}/[action]/{measureId}")]
        public async Task<IActionResult> BodyWeights(DeletePhysiologicalMeasureInfo input)
        {
            await DeleteOneMeasureAsync<BodyWeight>(input);
            return new NoContentResult();
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
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        private async Task DeleteOneMeasureAsync<TPhysiologicalMeasure>(DeletePhysiologicalMeasureInfo input, CancellationToken cancellationToken = default(CancellationToken))
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            => await _physiologicalMeasureService.DeleteOnePhysiologicalMeasureAsync<TPhysiologicalMeasure>(new DeleteOnePhysiologicalMeasureCommand(input), cancellationToken);

        /// <summary>
        /// Gets one of the patient's prescription
        /// </summary>
        /// <param name="id">Id of the patient</param>
        /// <param name="prescriptionId">Identifier of the prescription to get</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="200">if the prescription was found</response>
        /// <response code="404">no prescription with the <paramref name="prescriptionId"/> found.</response>
        /// <response code="404">no patient with the <paramref name="id"/> found</response>
        [HttpGet("{id}/[action]/{prescriptionId}")]
        [HttpHead("{id}/[action]/{prescriptionId}")]
        [ProducesResponseType(typeof(PrescriptionHeaderInfo), 200)]
        public async Task<IActionResult> Prescriptions(Guid id, Guid prescriptionId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Option<PrescriptionHeaderInfo> output = await _prescriptionService.GetOnePrescriptionByPatientIdAsync(id, prescriptionId, cancellationToken);

            return output.Match<IActionResult>(
                some: x =>
                   {
                       x.Meta = new Link
                       {
                           Relation = "self",
                           Href = UrlHelper.Action(nameof(Prescriptions), EndpointName, new { id = x.PatientId, prescriptionId = x.Id })
                       };
                       IBrowsableResource<PrescriptionHeaderInfo> browsableResource = new BrowsableResource<PrescriptionHeaderInfo>
                       {
                           Links = new[]
                           {
                               new Link
                                {
                                    Relation = nameof(Prescription.Items),
                                    Href = UrlHelper.Action(nameof(PrescriptionsController.Details), PrescriptionsController.EndpointName , new { x.Id })
                                }
                           },
                           Resource = x
                       };

                       return new OkObjectResult(browsableResource);
                   },
                none: () => new NotFoundResult());
        }


        /// <summary>
        /// Create a new prescription for a patient
        /// </summary>
        /// <param name="id">id of the patient</param>
        /// <param name="newPrescription">prescription details</param>
        /// <returns></returns>
        /// <response code="201">The header of the prescription.</response>
        /// <response code="400">if <paramref name="newPrescription"/> contains invalid data.</response>
        [HttpPost("{id}/[action]")]
        [ProducesResponseType(typeof(PrescriptionHeaderInfo), 201)]
        public async Task<IActionResult> Prescriptions(Guid id, [FromBody] CreatePrescriptionInfo newPrescription)
        {
            PrescriptionHeaderInfo createdPrescription = await _prescriptionService.CreatePrescriptionForPatientAsync(id, newPrescription);

            IBrowsableResource<PrescriptionHeaderInfo> browsableResource = new BrowsableResource<PrescriptionHeaderInfo>
            {
                Resource = createdPrescription,
                Links = new[]
                {
                    new Link {
                        Href = UrlHelper.Action(nameof(PrescriptionsController.Details), PrescriptionsController.EndpointName, new {createdPrescription.Id}),
                        Method = "GET",
                        Relation = nameof(Prescription.Items),
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
        [HttpGet("{id}/[action]")]
        [HttpHead("{id}/[action]")]
        [ProducesResponseType(typeof(IEnumerable<PrescriptionHeaderInfo>), 200)]
        public async Task<IActionResult> MostRecentPrescriptions(Guid id, [FromQuery] int? count)
        {
            GetMostRecentPrescriptionsInfo input = new GetMostRecentPrescriptionsInfo { PatientId = id, Count = count };
            Option<IEnumerable<PrescriptionHeaderInfo>> prescriptions = await _prescriptionService.GetMostRecentPrescriptionsAsync(new WantMostRecentPrescriptionsQuery(input));

            return prescriptions.Match<IActionResult>(
                some: x => new OkObjectResult(x),
                none: () => new NotFoundResult());
        }


        /// <summary>
        /// Adds a new <see cref="BodyWeightInfo"/> for the patient with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">id of the patient the measure will be created for</param>
        /// <param name="newBodyWeight">measure to add</param>
        /// <response code="201">the resource created successfully</response>
        /// <response code="400"><paramref name="newBodyWeight"/> is not valid or <paramref name="id"/> is negative or zero</response>
        [HttpPost("{id}/[action]")]
        [ProducesResponseType(typeof(BodyWeightInfo), 200)]
        public async Task<IActionResult> BodyWeights(Guid id, [FromBody] CreateBodyWeightInfo newBodyWeight)
        {

            CreatePhysiologicalMeasureInfo<BodyWeight> input = new CreatePhysiologicalMeasureInfo<BodyWeight>
            {
                PatientId = id,
                Measure = new BodyWeight
                {
                    DateOfMeasure = newBodyWeight.DateOfMeasure,
                    Value = newBodyWeight.Value
                }
            };
            BodyWeightInfo output = await _physiologicalMeasureService.AddNewMeasureAsync<BodyWeight, BodyWeightInfo>(new AddNewPhysiologicalMeasureCommand<BodyWeight, BodyWeightInfo>(input));
            return new CreatedAtActionResult(nameof(BodyWeights), EndpointName, new { id = output.PatientId, bodyWeightId = output.Id }, output);
        }

        /// <summary>
        /// Generates additional id for the patient resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        protected override IEnumerable<Link> BuildAdditionalLinksForResource(PatientInfo resource)
            => resource.MainDoctorId.HasValue
                ? new[]
                {
                    new Link {
                        Relation = "main-doctor",
                        Href = UrlHelper.Action(nameof(DoctorsController.Get), DoctorsController.EndpointName, new { id = resource.MainDoctorId })
                    },
                    new Link
                    {
                        Relation = "delete",
                        Method = "DELETE",
                        Href = UrlHelper.Action(nameof(PatientsController.Delete), EndpointName, new { id = resource.Id })
                    },
                    new Link
                    {
                        Relation = nameof(Documents).ToLower(),
                        Href = UrlHelper.Action(nameof(PatientsController.Documents), EndpointName, new { id = resource.Id })
                    },
                    new Link
                    {
                        Relation = nameof(PatientsController.MostRecentTemperatures).ToLowerKebabCase(),
                        Href = UrlHelper.Action(nameof(PatientsController.MostRecentTemperatures), EndpointName, new { id = resource.Id })
                    },
                    new Link
                    {
                        Relation = nameof(PatientsController.MostRecentBloodPressures).ToLowerKebabCase(),
                        Href = UrlHelper.Action(nameof(PatientsController.MostRecentBloodPressures), EndpointName, new { id = resource.Id })
                    }
                }
                : Enumerable.Empty<Link>();


        /// <summary>
        /// Gets all patient's documents metadata
        /// </summary>
        /// <remarks>
        /// This method gets all documents' metadata that are related to the patient with the specified <paramref name="id"/>.
        /// Documents are sorted by their last updated date descending.
        /// </remarks>
        /// <param name="id">id of the patient to get documents from</param>
        /// <param name="page">Index of the page of result set (the first page is 1).</param>
        /// <param name="pageSize">Size of a page of results.</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="200">The documents' metadata.</response>
        /// <response code="404">if no patient found.</response>
        [HttpGet("{id}/[action]")]
        [ProducesResponseType(typeof(IEnumerable<DocumentMetadataInfo>), 200)]
        public async Task<IActionResult> Documents(Guid id, [FromQuery]int page, [FromQuery]int pageSize, CancellationToken cancellationToken = default(CancellationToken))
        {
            PaginationConfiguration query = new PaginationConfiguration
            {
                Page = page,
                PageSize = Math.Min(ApiOptions.Value.MaxPageSize, pageSize)
            };

            Option<IPagedResult<DocumentMetadataInfo>> result = await _iHandleGetDocumentByPatientIdQuery.HandleAsync(new WantDocumentsByPatientIdQuery(id, query), cancellationToken);



            return await result.Match(
                some: x =>
                  Task.Run<IActionResult>(async () =>
                   {

                       int count = x.Entries.Count();
                       bool hasPreviousPage = count > 0 && query.Page > 1;

                       string firstPageUrl = UrlHelper.Action(nameof(Documents), ControllerName, new { PageSize = query.PageSize, Page = 1, id });
                       string previousPageUrl = hasPreviousPage
                               ? UrlHelper.Action(nameof(Documents), ControllerName, new { PageSize = query.PageSize, Page = query.Page - 1, id, })
                               : null;

                       string nextPageUrl = query.Page < x.PageCount
                               ? UrlHelper.Action(nameof(Documents), ControllerName, new { PageSize = query.PageSize, Page = query.Page + 1, id })
                               : null;
                       string lastPageUrl = x.PageCount > 0
                               ? UrlHelper.Action(nameof(Documents), ControllerName, new { PageSize = query.PageSize, Page = x.PageCount, id })
                               : null;

                       await x.Entries.ForEachAsync((item) => Task.Run(() => item.Meta = new Link { Href = UrlHelper.Action(nameof(Get), new { item.Id }) }));


                       IGenericPagedGetResponse<DocumentMetadataInfo> response = new GenericPagedGetResponse<DocumentMetadataInfo>(
                           x.Entries,
                           firstPageUrl,
                           previousPageUrl,
                           nextPageUrl,
                           lastPageUrl,
                           x.Total);

                       return new OkObjectResult(response);
                   }),
                none: () => new ValueTask<IActionResult>(new NotFoundResult()).AsTask());
        }


        /// <summary>
        /// Creates a new <paramref name="document"/> and attaches it to the patient resource with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">id of the patient resource <paramref name="document"/> must be attached the to.</param>
        /// <param name="document">The file to upload</param>
        /// <returns></returns>
        /// <response code="201">the document metadata</response>
        /// <response code="400">Invalid data sent (no binary content, missing required field(s), ...)</response>
        /// <response code="404">No patient found</response>
        [HttpPost("{id}/[action]")]
        [ProducesResponseType(typeof(DocumentMetadataInfo), 201)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Documents(Guid id, [FromForm] IFormFile document)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await document.CopyToAsync(ms);
                CreateDocumentInfo documentInfo = new CreateDocumentInfo
                {
                    MimeType = document.ContentType,
                    Content = ms.ToArray(),
                    Title = document.Name
                };
                CreateDocumentForPatientCommand cmd = new CreateDocumentForPatientCommand(id, documentInfo);
                Option<DocumentMetadataInfo, CommandException> resource = await _iRunCreateDocumentForPatientCommand.RunAsync(cmd);

                return resource.Match(
                    some: doc =>
                   {
                       IBrowsableResource<DocumentMetadataInfo> browsableResource = new BrowsableResource<DocumentMetadataInfo>
                       {
                           Resource = doc,
                           Links = new[]
                            {
                                new Link { Relation = "direct-link" },
                                new Link { Relation = "file" }
                            }
                       };

                       return new CreatedAtActionResult(
                            nameof(Documents),
                            EndpointName,
                            new { id, documentId = doc.Id },
                            browsableResource);
                   },
                    none: exception =>
                   {
                       IActionResult result;
                       switch (exception)
                       {
                           case CommandEntityNotFoundException cenf:
                               result = new NotFoundObjectResult(cenf.Message);
                               break;
                           default:
                               result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                               break;
                       }

                       return result;
                   });
            }
        }


        /// <summary>
        /// Gets the specified document that is associated to the specified patient
        /// </summary>
        /// <param name="id">id of the patient to </param>
        /// <param name="documentMetadataId">id of the document to get</param>
        /// <param name="cancellationToken">Notifies lower layers about the request abortion</param>
        /// <returns></returns>
        /// <response code="404">no patient/document found</response>
        /// <response code="200">The document</response>
        [HttpGet("{id}/[action]/{documentMetadataId}")]
        [ProducesResponseType(typeof(DocumentMetadataInfo), 200)]
        public async Task<IActionResult> Documents(Guid id, Guid documentMetadataId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Option<DocumentMetadataInfo> resource = await _iHandleGetOneDocumentInfoByPatientIdAndDocumentId
                .HandleAsync(new WantOneDocumentByPatientIdAndDocumentIdQuery(id, documentMetadataId), cancellationToken);

            return resource.Match<IActionResult>(
                some: x =>
           {
               IBrowsableResource<DocumentMetadataInfo> browsableResource = new BrowsableResource<DocumentMetadataInfo>
               {
                   Resource = x,
                   Links = new[]
                   {
                        new Link { Relation = "file", Href = UrlHelper.Action(nameof(DocumentsController.File), DocumentsController.EndpointName, new { x.Id }) },
                        new Link { Relation = "direct-link", Href = UrlHelper.Action(nameof(DocumentsController.Get), DocumentsController.EndpointName, new { x.Id }) }
                   }
               };

               return new OkObjectResult(browsableResource);
           },
            none: () => new NotFoundResult());
        }

    }
}
