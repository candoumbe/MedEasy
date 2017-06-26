using Microsoft.Extensions.DependencyInjection;
using MedEasy.Handlers.Specialty.Queries;
using MedEasy.Handlers.Specialty.Commands;
using MedEasy.API.Controllers;
using MedEasy.Commands.Specialty;
using MedEasy.Validators;
using MedEasy.Handlers.Core.Specialty.Queries;
using MedEasy.Handlers.Core.Specialty.Commands;

namespace MedEasy.API.StartupRegistration
{
    /// <summary>
    /// Wraps the dependency injection registration for <see cref="SpecialtiesController"/> dependencies.
    /// </summary>
    public static class SpecialtyStartupRegistration
    {
        /// <summary>
        /// Registers all dependencies needed by <see cref="SpecialtiesController"/>
        /// </summary>
        public static void AddSpecialtiesControllerDependencies(this IServiceCollection services)
        {

            services.AddScoped<IValidate<ICreateSpecialtyCommand>>(x => Validator<ICreateSpecialtyCommand>.Default);
            services.AddScoped<IValidate<IDeleteSpecialtyByIdCommand>>(x => Validator<IDeleteSpecialtyByIdCommand>.Default);

            services.AddScoped<IHandleGetSpecialtyInfoByIdQuery, HandleGetSpecialtyInfoByIdQuery>();
            services.AddScoped<IHandleGetPageOfSpecialtyInfosQuery, HandleGetPageOfSpecialtyInfoQuery>();
            services.AddScoped<IHandleIsNameAvailableForNewSpecialtyQuery, HandleIsNameAvailableForNewSpecialtyQuery>();
            services.AddScoped<IHandleFindDoctorsBySpecialtyIdQuery, HandleFindDoctorsBySpecialtyIdQuery>();

            services.AddScoped<IRunCreateSpecialtyCommand, RunCreateSpecialtyCommand>();
            services.AddScoped<IRunDeleteSpecialtyByIdCommand, RunDeleteSpecialtyByIdCommand>();


        }
    }
}
