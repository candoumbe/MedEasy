using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using MedEasy.Validators;
using MedEasy.Validators.Exceptions;
using MedEasy.Handlers.Core.Exceptions;
using static Microsoft.AspNetCore.Http.StatusCodes;

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
        /// <summary>
        /// <see cref="ExceptionFilterAttribute.OnExceptionAsync(ExceptionContext)"/>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task OnExceptionAsync(ExceptionContext context)
        {

            Exception exception = context.Exception;
            Type exceptionType = exception.GetType();

            switch (exception)
            {
                case CommandException ce:
                    IEnumerable<ErrorInfo> errors = null;
                    IEnumerable<PropertyInfo> properties = null;
                    Type commandNotValidExceptionType = typeof(CommandNotValidException<>);

                    if (exceptionType.IsAssignableToGenericType(commandNotValidExceptionType))
                    {
                        commandNotValidExceptionType = commandNotValidExceptionType.MakeGenericType(exceptionType.GetGenericArguments()[0]);
                        properties = commandNotValidExceptionType.GetProperties();
                        PropertyInfo piErrors = properties.Single(x => x.CanRead && x.Name == nameof(CommandException.Errors));
                        PropertyInfo piCommandId = properties.Single(x => x.CanRead && x.Name == nameof(CommandNotValidException<int>.CommandId));
                        Guid commandId = (Guid)piCommandId.GetValue(exception);
                        errors = (IEnumerable<ErrorInfo>)piErrors.GetValue(exception);
                        _logger.LogError($"Command '{commandId}' is not valid");

                        context.ExceptionHandled = true;
                        context.Result = new BadRequestObjectResult(errors);
                    }
                    else
                    {
                        Type commandConflictException = typeof(CommandConflictException<>);
                        if (exceptionType.IsAssignableToGenericType(commandConflictException))
                        {
                            commandConflictException = commandConflictException.MakeGenericType(exceptionType.GetGenericArguments()[0]);
                            properties = commandConflictException.GetProperties();
                            PropertyInfo piErrors = properties.Single(x => x.CanRead && x.Name == nameof(ValidationException.Errors));
                            PropertyInfo piCommandId = properties.Single(x => x.CanRead && x.Name == nameof(CommandConflictException<int>.CommandId));
                            Guid commandId = (Guid)piCommandId.GetValue(exception);
                            errors = (IEnumerable<ErrorInfo>)piErrors.GetValue(exception);
                            _logger.LogError($"Command '{commandId}' conflict");

                            context.ExceptionHandled = true;
                            context.Result = new StatusCodeResult(Status409Conflict);
                        }
                        else
                        {
                            _logger.LogError($"Command exception");
                            context.ExceptionHandled = true;
                            context.Result = new ObjectResult(new {ce.Message, ce.Errors }) { StatusCode = Status500InternalServerError };
                        }
                    }

                    foreach (ErrorInfo error in errors)
                    {
                        context.ModelState.TryAddModelError(error.Key, error.Description);
                    }
                    break;
                case QueryException qe:

                    if (qe is QueryNotFoundException qnfe)
                    {
                        context.Result = new NotFoundObjectResult(qnfe.Message);
                    }
                    else
                    {
                        context.Result = new BadRequestObjectResult(new { qe.Message, qe.Errors });
                    }

                    context.ExceptionHandled = true;
                    foreach (ErrorInfo error in qe.Errors)
                    {
                        context.ModelState.TryAddModelError(error.Key, error.Description);
                    }
                    break;
                default:
                    context.ExceptionHandled = true;
                    context.Result = new ObjectResult(exception.Message) { StatusCode = Status500InternalServerError };
                    break;
            }

            await base.OnExceptionAsync(context);

        }
    }
}