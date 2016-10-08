using MedEasy.DTO;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using MedEasy.RestObjects;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;
using MedEasy.Tools.Extensions;

namespace MedEasy.API.Filters
{
    public class EnvelopeFilterAttribute : ActionFilterAttribute
    {


        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            object result = context.Result;
            if (result is OkObjectResult)
            {
                OkObjectResult okObjectResult = (OkObjectResult) result;
                object value = okObjectResult.Value;
                Type valueType = value.GetType();
                Type typeBrowsable = typeof(IBrowsableResource<>);
                if (valueType.IsAssignableToGenericType(typeBrowsable))
                {

                    typeBrowsable = typeBrowsable.MakeGenericType(valueType.GetGenericArguments()[0]);
                    IEnumerable<PropertyInfo> properties = typeBrowsable.GetProperties();
                    PropertyInfo piLocation = properties
                        .SingleOrDefault(x => x.Name == nameof(IBrowsableResource<object>.Location));

                    if (piLocation != null && piLocation.CanRead)
                    {
                        Link location = (Link)piLocation.GetValue(value);
                        context.HttpContext.Response.Headers.Add("Link", $@"""{location.Href}"";{(string.IsNullOrWhiteSpace(location.Rel) ? string.Empty : $@" rel=""{location.Rel}""")}");
                    }
                } 
            }
        }
    }
}
