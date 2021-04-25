using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using Patients.DTO;
using Patients.Validators.Features.Patients.DTO;

namespace Patients.API.StartupRegistration
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
            services.AddScoped<IValidator<CreatePatientInfo>, CreatePatientInfoValidator>();
        }
    }
}
