using Identity.API.Features.Accounts;
using Identity.API.Routing;
using Identity.DTO;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using static MedEasy.RestObjects.FormFieldType;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Identity.API.Features
{
    /// <summary>
    /// Controller that describe 
    /// </summary>
    [Controller]
    [Route("/")]
    [AllowAnonymous]
    public class RootController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUrlHelper _urlHelper;
        private IOptions<IdentityApiOptions> ApiOptions { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingEnvironment">Gives access to hosting environment</param>
        /// <param name="urlHelper"></param>
        /// <param name="apiOptions">Gives access to the API configuration</param>
        /// 
        public RootController(IHostingEnvironment hostingEnvironment, IUrlHelper urlHelper, IOptions<IdentityApiOptions> apiOptions)
        {
            _hostingEnvironment = hostingEnvironment;
            _urlHelper = urlHelper;
             ApiOptions = apiOptions;
        }


        /// <summary>
        /// Describes all endpoints
        /// </summary>
        /// <remarks>
        /// 
        ///     API clients should only relies on link's relation to navigate through all 
        ///     resources returned by this API
        ///
        /// </remarks>
        /// <response code="200"></response>
        [HttpGet]
        [HttpOptions]
        [HttpHead]
        [ProducesResponseType(typeof(IEnumerable<Endpoint>), 200)]
        public IEnumerable<Endpoint> Index()
        {
            IdentityApiOptions apiOptions = ApiOptions.Value;
            int page = 1,
                pageSize = apiOptions.DefaultPageSize,
                maxPageSize = apiOptions.MaxPageSize;
            IList<Endpoint> endpoints = new List<Endpoint>() {
                new Endpoint
                {
                    Name = AccountsController.EndpointName.ToLowerKebabCase(),
                    Link = new Link
                    {
                        Title = "Collection of accounts",
                        Method = "GET",
                        Relation = LinkRelation.Collection,
                        Href = _urlHelper.Link(RouteNames.DefaultGetAllApi, new {controller = AccountsController.EndpointName, page, pageSize})
                    },
                    Forms = new[]
                    {
                        new FormBuilder<SearchAccountInfo>(new Link { Relation = LinkRelation.Search, Method = "GET", Href = _urlHelper.Link(RouteNames.DefaultSearchResourcesApi, new { controller = AccountsController.EndpointName})  })
                            .AddField(x => x.UserName)
                            .AddField(x => x.Page)
                            .AddField(x => x.PageSize, new FormFieldAttributeOverrides { Max = apiOptions.MaxPageSize })
                            .AddField(x => x.Sort)
                            .Build()
                    }
                },

            };

            if (!_hostingEnvironment.IsProduction())
            {
                endpoints.Add(new Endpoint
                {
                    Name = "documentation",
                    Link = new Link
                    {
                        Href = _urlHelper.Link("default", new { controller = "swagger" }),
                        Relation = "help"
                    }
                });
            }

            return endpoints.OrderBy(x => x.Name);
        }
    }
}
