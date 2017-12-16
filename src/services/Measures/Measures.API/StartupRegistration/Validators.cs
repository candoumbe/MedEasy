using FluentValidation;
using Measures.DTO;
using Measures.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Measures.API.StartupRegistration
{
    /// <summary>
    /// Helper class to register validators.
    /// </summary>
    public static class Validators
    {
        /// <summary>
        /// Registers validators into the D.I
        /// </summary>
        /// <param name="services"></param>
        public static void AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<CreateBloodPressureInfo>, CreateBloodPressureInfoValidator>();
        }
    }
}
