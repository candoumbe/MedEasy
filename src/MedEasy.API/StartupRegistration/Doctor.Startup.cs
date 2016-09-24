using Microsoft.Extensions.DependencyInjection;
using MedEasy.Handlers.Doctor.Queries;
using MedEasy.Handlers.Doctor.Commands;
using MedEasy.API.Controllers;

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


        }

    }
}
