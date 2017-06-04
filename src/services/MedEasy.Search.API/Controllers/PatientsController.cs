using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MedEasy.DTO.Search;
using MedEasy.Data;
using MedEasy.DTO;
using MedEasy.DAL.Repositories;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static MedEasy.Data.DataFilterLogic;
using Microsoft.Extensions.Options;
using System.Threading;
using MedEasy.Handlers.Core.Search.Queries;
using MedEasy.Objects;
using MedEasy.RestObjects;
using MedEasy.Queries.Search;

namespace MedEasy.Search.API.Controllers
{
    [Route("api/[controller]")]
    public class PatientsController
    {

        private static string ControllerName => nameof(PatientsController)
            .Replace("Controller", string.Empty);


        /// <summary>
        /// Handler for search queries
        /// </summary>
        private readonly IHandleSearchQuery _iHandleSearchQuery;

        /// <summary>
        /// Helper to build URLs
        /// </summary>
        private readonly IUrlHelper _urlHelper;

        private IOptionsSnapshot<ApiOptions> ApiOptions { get; }
        /// <summary>
        /// Builds a new <see cref="PatientsController"/> instance
        /// </summary>
        /// <param name="apiOptions">Options defines in the appsettings.json file</param>
        /// <param name="iHandleSearchQuery">Handler for search queries</param>
        /// <param name="urlHelper">Helper to build URLs</param>
        public PatientsController(IUrlHelper urlHelper, IHandleSearchQuery iHandleSearchQuery, IOptionsSnapshot<ApiOptions> apiOptions)
        {
            ApiOptions = apiOptions;
            _iHandleSearchQuery = iHandleSearchQuery;
            _urlHelper = urlHelper;
        }


        /// <summary>
        /// Search patients resource based on some criteria.
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <param name="cancellationToken">Notification to abort the search.</param>
        /// <remarks>
        /// All criteria are combined as a AND.
        /// 
        /// Advanded search :
        /// Several operators that can be used to make an advanced search :
        /// '*' : match zero or more characters in a string property.
        /// 
        ///     // GET api/Patients/Search?Firstname=Bruce HTTP/1.1
        ///     will match all resources which have exactly 'Bruce' in the Firstname property
        ///     
        ///     // GET api/Patients/Search?Firstname=B*e
        ///     will match match all resources which starts with 'B' and ends with 'e'.
        /// 
        /// '?' : match exactly one charcter in a string property.
        /// 
        /// '!' : negate a criteria
        /// 
        ///     // GET api/Patients/Search?Firstname=!Bruce HTTP/1.1
        ///     will match all resources where Firstname is not "Bruce"
        ///     
        /// </remarks>
        /// <response code="200">Array of resources that matches <paramref name="search"/> criteria.</response>
        /// <response code="400">one the search criteria is not valid</response>
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(IEnumerable<PatientInfo>), 200)]
        [ProducesResponseType(typeof(IEnumerable<ModelStateEntry>), 400)]
        public async Task<IActionResult> Search([FromQuery] SearchPatientInfo search, CancellationToken cancellationToken = default(CancellationToken))
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


            IPagedResult<PatientInfo> pageOfResult = await _iHandleSearchQuery.Search<Patient, PatientInfo>(new SearchQuery<PatientInfo>(searchQueryInfo), cancellationToken);

            search.PageSize = Math.Min(search.PageSize, ApiOptions.Value.MaxPageSize);
            int count = pageOfResult.Entries.Count();
            bool hasPreviousPage = count > 0 && search.Page > 1;

            
            string firstPageUrl = _urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, search.BirthDate, Page = 1, search.PageSize, search.Sort });
            string previousPageUrl = hasPreviousPage
                    ? _urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, search.BirthDate, Page = search.Page - 1, search.PageSize, search.Sort })
                    : null;

            string nextPageUrl = search.Page < pageOfResult.PageCount
                    ? _urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, search.BirthDate, Page = search.Page + 1, search.PageSize, search.Sort })
                    : null;
            string lastPageUrl = pageOfResult.PageCount > 1
                    ? _urlHelper.Action(nameof(Search), ControllerName, new { search.Firstname, search.Lastname, search.BirthDate, Page = pageOfResult.PageCount, search.PageSize, search.Sort })
                    : null;

            //await pageOfResult.Entries.ForEachAsync((x) => Task.FromResult(x.Meta = new Link { Href = _urlHelper.Action(nameof(Get), new { x.Id }) }));


            IGenericPagedGetResponse<PatientInfo> reponse = new GenericPagedGetResponse<PatientInfo>(
                pageOfResult.Entries,
                first: firstPageUrl,
                previous: previousPageUrl,
                next: nextPageUrl,
                last: lastPageUrl,
                count: pageOfResult.Total);

            return new OkObjectResult(reponse);

        }

    }
}
