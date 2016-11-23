using MedEasy.Handlers.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using MedEasy.Validators;
using MedEasy.Tools.Extensions;
using MedEasy.Validators.Exceptions;

namespace MedEasy.API.Filters
{
    /// <summary>
    /// Attribute to apply to handle exceptions
    /// </summary>
    public class HandleErrorAttribute : ExceptionFilterAttribute
    {
        private ILogger<HandleErrorAttribute> _logger;

        /// <summary>
        /// Builds a new <see cref="HandleErrorAttribute"/> instance
        /// </summary>
        /// <param name="logger"></param>
        public HandleErrorAttribute(ILogger<HandleErrorAttribute> logger)
        {
            _logger = logger;
        }

        public override Task OnExceptionAsync(ExceptionContext context)
        {
            Exception exception = context.Exception;
            Type exceptionType = exception.GetType();
            Type validationExceptionType = typeof(ValidationException);
            if (validationExceptionType.IsAssignableFrom(exceptionType))
            {
                IEnumerable<ErrorInfo> errors = null;
                IEnumerable<PropertyInfo> properties = null;
                Type commandNotValidExceptionType = typeof(CommandNotValidException<>);

                bool isCommandException = exceptionType.IsAssignableToGenericType(commandNotValidExceptionType);
                if (isCommandException)
                {
                    commandNotValidExceptionType = commandNotValidExceptionType.MakeGenericType(exceptionType.GetGenericArguments()[0]);
                    properties = commandNotValidExceptionType.GetProperties();
                    PropertyInfo piErrors = properties.Single(x => x.CanRead && x.Name == nameof(ValidationException.Errors));
                    PropertyInfo piCommandId = properties.Single(x => x.CanRead && x.Name == nameof(CommandNotValidException<int>.CommandId));
                    Guid commandId = (Guid)piCommandId.GetValue(exception);
                    errors = (IEnumerable<ErrorInfo>)piErrors.GetValue(exception);
                    _logger.LogError($"Command '{commandId}' is not valid");
                }
                else
                {
                    Type queryNotValidExceptionType = typeof(QueryNotValidException<>);
                    queryNotValidExceptionType = queryNotValidExceptionType.MakeGenericType(exceptionType.GetGenericArguments()[0]);
                    properties = queryNotValidExceptionType.GetProperties();
                    PropertyInfo piErrors = properties.Single(x => x.CanRead && x.Name == nameof(ValidationException.Errors));
                    PropertyInfo piQueryId = properties.Single(x => x.CanRead && x.Name == nameof(QueryNotValidException<int>.QueryId));
                    Guid queryId = (Guid)piQueryId.GetValue(exception);
                    errors = (IEnumerable<ErrorInfo>)piErrors.GetValue(exception);

                    _logger.LogError($"Query '{queryId}' is not valid");
                }

                context.Result = new BadRequestObjectResult(errors);
                context.ExceptionHandled = true;
            }
            else if (context.Exception is NotFoundException)
            {
                context.ExceptionHandled = true;
                context.Result = new NotFoundObjectResult(context.Exception.Message);
            }

            return base.OnExceptionAsync(context);
        }
    }
}
