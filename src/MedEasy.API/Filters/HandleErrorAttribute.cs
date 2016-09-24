using MedEasy.Handlers.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.API.Filters
{
    public class HandleErrorAttribute : ExceptionFilterAttribute
    {
        public override Task OnExceptionAsync(ExceptionContext context)
        {
            Exception exception = context.Exception;

            if (context.Exception.GetType() == typeof(CommandNotValidException<>))
            {
                
            }

            return Task.CompletedTask;
        }
    }
}
