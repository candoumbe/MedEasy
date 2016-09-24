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
using MedEasy.Handlers.Doctor.Queries;
using MedEasy.Queries.Doctor;
using MedEasy.Handlers.Doctor.Commands;
using MedEasy.Commands.Doctor;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Endpoint to handle CRUD operations on <see cref="DoctorInfo"/> resources
    /// </summary>
    [Route("api/[controller]")]
    public class DoctorsController : RestCRUDControllerBase<int, Doctor, DoctorInfo, IWantOneDoctorInfoByIdQuery, IWantManyDoctorInfoQuery, Guid, CreateDoctorInfo, ICreateDoctorCommand, IRunCreateDoctorCommand>
    {
        /// <summary>
        /// Name of the endpoint
        /// </summary>
        public static string EndpointName => nameof(DoctorsController).Replace("Controller", string.Empty);


        /// <summary>
        /// Name of the controller without the "Controller" suffix 
        /// </summary>
        protected override string ControllerName => EndpointName;

       

        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IRunCreateDoctorCommand _iRunCreateDoctorCommand;
        private readonly IRunDeleteDoctorInfoByIdCommand _iRunDeleteDoctorByIdCommand;

        /// <summary>
        /// Builds a new <see cref="DoctorsController"/> instance
        /// </summary>
        /// <param name="getByIdQueryHandler">Handler of GET one resource</param>
        /// <param name="getManyDoctorQueryHandler">Handler of GET many resources</param>
        /// <param name="iRunCreateDoctorCommand">Runner of CREATE resource command</param>
        /// <param name="iRunDeleteDoctorByIdCommand">Runner of DELETE resource command</param>
        /// <param name="logger">logger</param>
        /// <param name="urlHelperFactory">Factory used to build <see cref="IUrlHelper"/> instances.</param>
        /// <param name="actionContextAccessor"></param>
        public DoctorsController(ILogger<DoctorsController> logger, IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor, 
            IOptions<MedEasyApiOptions> apiOptions,
            IHandleGetDoctorInfoByIdQuery getByIdQueryHandler,
            IHandleGetManyDoctorInfosQuery getManyDoctorQueryHandler,
            IRunCreateDoctorCommand iRunCreateDoctorCommand,
            IRunDeleteDoctorInfoByIdCommand iRunDeleteDoctorByIdCommand) : base(logger, apiOptions, getByIdQueryHandler, getManyDoctorQueryHandler, iRunCreateDoctorCommand, urlHelperFactory, actionContextAccessor)
        { 
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _iRunCreateDoctorCommand = iRunCreateDoctorCommand;
            _iRunDeleteDoctorByIdCommand = iRunDeleteDoctorByIdCommand;

        }


        /// <summary>
        /// Gets all the entries in the repository
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(GenericGetResponse<BrowsableDoctorInfo>))]
        public async Task<IActionResult> Get(GenericGetQuery query)
        {
            if (query == null)
            {
                query = new GenericGetQuery();
            }

            
                
            IPagedResult<DoctorInfo> result = await GetAll(query);
           
            
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


            IGetResponse<BrowsableDoctorInfo> response = new GenericPagedGetResponse<BrowsableDoctorInfo>(
                result.Entries.Select(x => 
                    new BrowsableDoctorInfo {
                        Location = new Link { Href = urlHelper.Action(nameof(DoctorsController.Get), ControllerName, new { Id = x.Id }) },
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
        /// Gets the <see cref="DoctorInfo"/> resource by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">identifier of the resource to look for</param>
        /// <returns></returns>
        [HttpHead("{id:int}")]
        [HttpGet("{id:int}")]
        [Produces(typeof(BrowsableDoctorInfo))]
        public async override Task<IActionResult> Get(int id) => await base.Get(id);
            

        
        /// <summary>
        /// Creates the resource
        /// </summary>
        /// <param name="info">data used to create the resource</param>
        /// <returns>the created resource</returns>
        [HttpPost]
        [Produces(typeof(BrowsableResource<DoctorInfo>))]
        public async Task<IActionResult> Post([FromBody] CreateDoctorInfo info)
        {
            DoctorInfo output = await _iRunCreateDoctorCommand.RunAsync(new CreateDoctorCommand(info));
            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            BrowsableDoctorInfo browsableResource = new BrowsableDoctorInfo
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
        [Produces(typeof(BrowsableDoctorInfo))]
        public async Task<IActionResult> Put(int id, [FromBody] DoctorInfo info)
        {
            throw new NotImplementedException();
        }

        // DELETE api/specialties/5

        /// <summary>
        /// Delete the <see cref="DoctorInfo"/> by its 
        /// </summary>
        /// <param name="id">identifier of the resource to delete</param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _iRunDeleteDoctorByIdCommand.RunAsync(new DeleteDoctorByIdCommand(id));
            return await Task.FromResult(new OkResult());
        }




    }
}
