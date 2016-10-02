using Microsoft.Extensions.DependencyInjection;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.API.Controllers;
using MedEasy.Commands.Patient;
using MedEasy.Validators;
using MedEasy.Queries;
using System;
using MedEasy.DTO;
using MedEasy.Objects;

namespace MedEasy.API.StartupRegistration
{
    /// <summary>
    /// Wraps the dependency injection registration for <see cref="SpecialtiesController"/> dependencies.
    /// </summary>
    public static class PatientStartupRegistration
    {
        /// <summary>
        /// Registers all dependencies needed by <see cref="PatientsController"/>
        /// </summary>
        public static void AddPatientsControllerDependencies(this IServiceCollection services)
        {

            services.AddScoped<IValidate<ICreatePatientCommand>>(x => Validator<ICreatePatientCommand>.Default);
            services.AddScoped<IValidate<IDeletePatientByIdCommand>>(x => Validator<IDeletePatientByIdCommand>.Default);
            services.AddScoped<IValidate<IWantOneResource<Guid, int, PatientInfo>>>(x => Validator<IWantOneResource<Guid, int, PatientInfo>>.Default);
            services.AddScoped<IValidate<IAddNewTemperatureMeasureCommand>>(x => Validator<IAddNewTemperatureMeasureCommand>.Default);
            
            services.AddScoped<IHandleGetOnePatientInfoByIdQuery, HandleGetPatientInfoByIdQuery>();
            services.AddScoped<IHandleGetManyPatientInfosQuery, HandleGetManyPatientInfoQuery>();
            services.AddScoped<IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo>, HandleGetOnePhysiologicalMeasurementInfoQuery<Temperature, TemperatureInfo>>();
            services.AddScoped<IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo>, HandleGetOnePhysiologicalMeasurementInfoQuery<BloodPressure,BloodPressureInfo>>();
            
            services.AddScoped<IRunCreatePatientCommand, RunCreatePatientCommand>();
            services.AddScoped<IRunDeletePatientByIdCommand, RunDeletePatientByIdCommand>();
            services.AddScoped<IRunAddNewTemperatureMeasureCommand, RunAddNewTemperatureMeasureCommand>();


        }
    }
}
