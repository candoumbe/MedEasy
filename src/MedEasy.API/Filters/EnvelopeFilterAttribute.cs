using MedEasy.DTO;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Mvc;
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
            IActionResult result = context.Result;
            if (result is ObjectResult)
            {
                object value = ((ObjectResult)result).Value;
                Type valueType = value.GetType();
                Type typeBrowsable = typeof(IBrowsableResource<>);
                Type typePageOfResult = typeof(IGenericPagedGetResponse<>);
                if (valueType.IsAssignableToGenericType(typeBrowsable))
                {
                    typeBrowsable = typeBrowsable.MakeGenericType(valueType.GetGenericArguments()[0]);
                    IEnumerable<PropertyInfo> properties = typeBrowsable.GetProperties();
                    PropertyInfo piLinks = properties
                        .Single(x => x.CanRead && x.Name == nameof(IBrowsableResource<object>.Links));
                    PropertyInfo piResource = properties
                        .Single(x => x.Name == nameof(IBrowsableResource<object>.Resource));


                    object resource = piResource.GetValue(value);
                    IEnumerable<Link> links = (IEnumerable<Link>)piLinks.GetValue(value);
                    StringBuilder sbLinks = new StringBuilder();
                    foreach (Link link in links)
                    {
                        sbLinks.Append($"{(sbLinks.Length > 0 ? ", " : string.Empty)}{BuildLinkHeader(link)}");
                    }
                    context.HttpContext.Response.Headers.Add("Link", sbLinks.ToString());
                    ((ObjectResult)result).Value = resource;
                }
                else if (valueType.IsAssignableToGenericType(typePageOfResult))
                {
                    typePageOfResult = typePageOfResult.MakeGenericType(valueType.GetGenericArguments()[0]);
                    IEnumerable<PropertyInfo> properties = typePageOfResult.GetProperties();

                    PropertyInfo piResources = properties.Single(x => x.Name == nameof(IGenericPagedGetResponse<object>.Items));
                    PropertyInfo piLinks = properties.Single(x => x.Name == nameof(IGenericPagedGetResponse<object>.Links));
                    PropertyInfo piCount = properties.Single(x => x.Name == nameof(IGenericPagedGetResponse<object>.Count));

                    object resources = piResources.GetValue(value);

                    // context.Result = new OkObjectResult(resources); //extract the resource
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
                    ((ObjectResult)result).Value = resources;
                }
            }

            base.OnResultExecuting(context);
        }
    }
}