using MedEasy.DTO;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc;
using MedEasy.Tools.Extensions;
using System.Text;

namespace MedEasy.API.Filters
{
    /// <summary>
    /// This filter attributes updates the response header with informations on the resource.
    /// </summary>
    /// <remarks>
    /// The filter is activated on actions that returns <see cref="IBrowsableResource{T}"/>  
    /// </remarks>
    public class EnvelopeFilterAttribute : ResultFilterAttribute, IResultFilter
    {

        private static Func<Link, string> BuildLinkHeader => location => location == null 
            ? string.Empty
            : $@"<{location.Href}>;{(string.IsNullOrWhiteSpace(location.Rel) ? string.Empty : $@" rel=""{location.Rel}""")}";

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
            IActionResult result = context.Result;
            if (result is OkObjectResult)
            {
                OkObjectResult okObjectResult = (OkObjectResult)result;
                object value = okObjectResult.Value;
                Type valueType = value.GetType();
                Type typeBrowsable = typeof(IBrowsableResource<>);
                Type typePageOfResult = typeof(GenericPagedGetResponse<>);
                if (valueType.IsAssignableToGenericType(typeBrowsable))
                {

                    typeBrowsable = typeBrowsable.MakeGenericType(valueType.GetGenericArguments()[0]);
                    IEnumerable<PropertyInfo> properties = typeBrowsable.GetProperties();
                    PropertyInfo piLocation = properties
                        .Single(x => x.CanRead && x.Name == nameof(IBrowsableResource<object>.Location));
                    PropertyInfo piResource = properties
                        .Single(x => x.Name == nameof(IBrowsableResource<object>.Resource));


                    object resource = piResource.GetValue(value);
                    context.Result = new OkObjectResult(resource); //extract the resource
                    Link location = (Link)piLocation.GetValue(value);
                    context.HttpContext.Response.Headers.Add("Link", BuildLinkHeader(location));

                }
                else if (valueType.IsAssignableToGenericType(typePageOfResult))
                {
                    typePageOfResult = typePageOfResult.MakeGenericType(valueType.GetGenericArguments()[0]);
                    IEnumerable<PropertyInfo> properties = typePageOfResult.GetProperties();

                    PropertyInfo piResources = properties.Single(x => x.Name == nameof(GenericPagedGetResponse<object>.Items));
                    PropertyInfo piLinks = properties.Single(x => x.Name == nameof(GenericPagedGetResponse<object>.Links));
                    PropertyInfo piCount = properties.Single(x => x.Name == nameof(GenericPagedGetResponse<object>.Count));

                    object resources = piResources.GetValue(value);
                    context.Result = new OkObjectResult(resources); //extract the resource
                    PagedRestResponseLink links = (PagedRestResponseLink)piLinks.GetValue(value);
                    StringBuilder sbLinks = new StringBuilder();
                    if (links.First != null)
                    {
                        sbLinks.Append(BuildLinkHeader(links.First));
                    }
                    if (links.Next != null)
                    {
                        sbLinks.Append(sbLinks.Length > 0 ? "," : string.Empty)
                            .Append(BuildLinkHeader(links.Next));
                    }
                    if (links.Previous != null)
                    {
                        sbLinks.Append(sbLinks.Length > 0 ? "," : string.Empty)
                            .Append(BuildLinkHeader(links.Previous));
                    }
                    if (links.Last != null)
                    {
                        sbLinks.Append(sbLinks.Length > 0 ? "," : string.Empty)
                            .Append(BuildLinkHeader(links.Last));
                    }
                    context.HttpContext.Response.Headers.Add("Link", sbLinks.ToString());
                    context.HttpContext.Response.Headers.Add("X-Total-Count", piCount.GetValue(value).ToString());


                }
            }
        }
    }
}
