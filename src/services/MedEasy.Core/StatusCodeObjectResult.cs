namespace MedEasy.Core
{
    using Microsoft.AspNetCore.Mvc;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    /// <summary>
    /// A <see cref="StatusCodeResult"/> implementation that can holds an additional value
    /// </summary>
    /// <remarks>
    /// This action result can be used when one needs to specify both a status code AND a body that will be serialize to HTTP response
    /// </remarks>
    public class StatusCodeObjectResult : StatusCodeResult
    {
        /// <summary>
        /// The value assigned to this instance
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Builds a new <see cref="StatusCodeObjectResult"/> instance with the given <paramref name="statusCode"/>
        /// and <paramref name="value"/>.
        /// </summary>
        /// <param name="statusCode"><see cref="StatusCodeResult.StatusCode"/></param>
        /// <param name="value">Defines the value of the instance</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public StatusCodeObjectResult(int statusCode, object value) : base(statusCode)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value;
        }
    }
}
