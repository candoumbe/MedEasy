using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedEasy.Web.Api;
using Microsoft.AspNetCore.Mvc;
using MedEasy.DTO;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading;

namespace MedEasy.Web.Controllers
{
    [Controller]
    [Route("/[controller]")]
    public class PatientsController
    {
        private readonly IPatientInfoCrudApi _patientApi;
        

        [ViewDataDictionary]
        public ViewDataDictionary ViewDataDictionary { get; set; }

        public PatientsController(IPatientInfoCrudApi patientApi) => _patientApi = patientApi;

        /// <summary>
        /// Gets the Index view.
        /// </summary>
        /// <returns><see cref="IActionResult"/> to display the view</returns>
        [HttpGet]
        public IActionResult Index() => new ViewResult();

        [HttpGet("/[action]")]
        public async Task<IActionResult> List(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            GenericPagedGetResponse<BrowsableResource<PatientInfo>> pageContent = await _patientApi.GetMany(new PaginationConfiguration { Page = page, PageSize = pageSize }, cancellationToken);

            ViewDataDictionary<GenericPagedGetResponse<BrowsableResource<PatientInfo>>> viewData = new ViewDataDictionary<GenericPagedGetResponse<BrowsableResource<PatientInfo>>>(ViewDataDictionary, pageContent);

            return new ViewResult
            {
                ViewData = viewData
            };
        }
    }
}