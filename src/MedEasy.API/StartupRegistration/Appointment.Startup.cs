using Microsoft.Extensions.DependencyInjection;
using MedEasy.Handlers.Appointment.Queries;
using MedEasy.Handlers.Appointment.Commands;
using MedEasy.API.Controllers;
using MedEasy.Handlers.Core.Appointment.Queries;
using MedEasy.Handlers.Core.Appointment.Commands;
using MedEasy.Validators;
using MedEasy.Commands;
using System;

namespace MedEasy.API.StartupRegistration
{
    /// <summary>
    /// Wraps the dependency injection registration for <see cref="AppointmentsController"/> dependencies.
    /// </summary>
    public static class AppointmentStartupRegistration
    {
        /// <summary>
        /// Registers all dependencies needed by <see cref="AppointmentsController"/>
        /// </summary>
        public static void AddAppointmentsControllerDependencies(this IServiceCollection services)
        {

            services.AddScoped<IHandleGetAppointmentInfoByIdQuery, HandleGetAppointmentInfoByIdQuery>();
            services.AddScoped<IHandleGetManyAppointmentInfosQuery, HandleGetManyAppointmentInfoQuery>();
            
            services.AddScoped<IRunCreateAppointmentCommand, RunCreateAppointmentCommand>();
            services.AddScoped<IRunDeleteAppointmentInfoByIdCommand, RunDeleteAppointmentByIdCommand>();
            services.AddScoped<IRunPatchAppointmentCommand, RunPatchAppointmentCommand>();

            services.AddScoped<IValidate<IPatchCommand<Guid, Objects.Appointment>>>(x => Validator<IPatchCommand<Guid, Objects.Appointment>>.Default);
        }

    }
}
