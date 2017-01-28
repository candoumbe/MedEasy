﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;

namespace MedEasy.API.Swagger
{
    /// <summary>
    /// <see cref="IOperationFilter"/> which allow to upload a file using Swagger
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Names of the parameters generated by swagger for a formFile
        /// </summary>
        private static IEnumerable<string> FormFilePropertyNames =>
            typeof(IFormFile).GetTypeInfo()
                .DeclaredProperties
                .Select(x => x.Name);
        /// <summary>
        /// Mime type for file uploads
        /// </summary>
        private static readonly string FormDataMimeType = "multipart/form-data";

        private static IEnumerable<Type> IntegerTypes
            => new[]
            {
                typeof(int),
                typeof(int?),
                typeof(long),
                typeof(long?),
                typeof(double),
                typeof(double?),
                typeof(short),
                typeof(short?),
                typeof(decimal),
                typeof(decimal?),
            };

        public void Apply(Operation operation, OperationFilterContext context)
        {

            ActionDescriptor action = context.ApiDescription.ActionDescriptor;
            IEnumerable<ParameterDescriptor> currentParameters = action.Parameters;

            ParameterDescriptor formFileParameterDescriptor = action.Parameters
                .FirstOrDefault(x => typeof(IFormFile).IsAssignableFrom(x.ParameterType));
            if (formFileParameterDescriptor != null)
            {
                IEnumerable<NonBodyParameter> formFileParameters = operation
                    .Parameters
                    .OfType<NonBodyParameter>()
                    .Where(x => FormFilePropertyNames.Contains(x.Name))
                    .ToArray();
                int index = operation.Parameters.IndexOf(formFileParameters.First());
                foreach (var formFileParameter in formFileParameters)
                {
                    operation.Parameters.Remove(formFileParameter);
                }


                operation.Parameters.Insert(index, new NonBodyParameter
                {
                    Name = formFileParameterDescriptor.Name,
                    In = "formData",
                    Description = $"{ formFileParameterDescriptor.Name } to upload",
                    Required = true,
                    Type = "file"
                });



                if (!operation.Consumes.Contains(FormDataMimeType))
                {
                    operation.Consumes.Add(FormDataMimeType);
                }
            }
        }
    }
}
