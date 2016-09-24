using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;

namespace MedEasy.Controllers
{
    /// <summary>
    /// Base class for all controllers of the application
    /// </summary>
    [Controller]
    public class BaseController
    {
        /// <summary>
        /// The <see cref="ILogger{TCategoryName}"/> instance used by the controller
        /// </summary>
        protected ILogger Logger { get; }

        private ViewDataDictionary _viewDataDictionary;

        [ViewDataDictionary]
        public ViewDataDictionary ViewData
        {
            get
            {
                if (_viewDataDictionary == null)
                {
                    _viewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
                }

                return _viewDataDictionary;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), $"value of ViewDataDictionary cannot be set to null");
                }
                _viewDataDictionary = value;
            }
        }


        protected BaseController(ILogger logger)
        {
            Logger = logger;
        }
    }
}
