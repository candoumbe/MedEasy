using Microsoft.Extensions.DependencyInjection;
using MedEasy.Handlers.Doctor.Queries;
using MedEasy.Handlers.Doctor.Commands;
using MedEasy.API.Controllers;
using MedEasy.Handlers.Core.Doctor.Queries;
using MedEasy.Handlers.Core.Doctor.Commands;
using MedEasy.Validators;
using MedEasy.Commands;
using System;

namespace MedEasy.API.StartupRegistration
{
    /// <summary>
    /// Wraps the dependency injection registration for <see cref="DoctorsController"/> dependencies.
    /// </summary>
    public static class DoctorStartupRegistration
    {
        /// <summary>
        /// Registers all dependencies needed by <see cref="DoctorsController"/>
        /// </summary>
        public static void AddDoctorsControllerDependencies(this IServiceCollection services)
        {

            services.AddScoped<IHandleGetDoctorInfoByIdQuery, HandleGetDoctorInfoByIdQuery>();
            services.AddScoped<IHandleGetManyDoctorInfosQuery, HandleGetManyDoctorInfoQuery>();
            
            services.AddScoped<IRunCreateDoctorCommand, RunCreateDoctorCommand>();
            services.AddScoped<IRunDeleteDoctorInfoByIdCommand, RunDeleteDoctorByIdCommand>();
            services.AddScoped<IRunPatchDoctorCommand, RunPatchDoctorCommand>();

            services.AddScoped<IValidate<IPatchCommand<Guid, Objects.Doctor>>>(x => Validator<IPatchCommand<Guid, Objects.Doctor>>.Default);



        }

    }
}
