using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MedEasy.API.Controllers
{
    /// <summary>
    /// Base class for all controllers of the application
    /// </summary>
    [Controller]
    public abstract class AbstractBaseController
    {
        /// <summary>
        /// The <see cref="ILogger{TCategoryName}"/> instance used by the controller
        /// </summary>
        protected ILogger Logger { get; }
        
        /// <summary>
        /// Builds a new <see cref="AbstractBaseController"/> instance
        /// </summary>
        /// <param name="logger">the logger</param>
        protected AbstractBaseController(ILogger logger)
        {
            Logger = logger;
        }
    }
}
