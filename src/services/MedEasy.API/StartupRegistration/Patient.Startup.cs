﻿using Microsoft.Extensions.DependencyInjection;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.API.Controllers;
using MedEasy.Commands.Patient;

using MedEasy.Validators;
using MedEasy.Queries;
using System;
using MedEasy.DTO;
using MedEasy.Objects;
using MedEasy.Services;
using MedEasy.Commands;
using MedEasy.Handlers.Core.Patient.Queries;
using MedEasy.Handlers.Core.Patient.Commands;
using MedEasy.Validators.Patient.Commands;
using FluentValidation;

namespace MedEasy.API.StartupRegistration
{
    /// <summary>
    /// Wraps the dependency injection registration for <see cref="PatientsController"/> dependencies.
    /// </summary>
    public static class PatientStartupRegistration
    {
        /// <summary>
        /// Registers all dependencies needed by <see cref="PatientsController"/>
        /// </summary>
        public static void AddPatientsControllerDependencies(this IServiceCollection services)
        {
            services.AddScoped<IValidator<ICreatePatientCommand>>(x => Validator<ICreatePatientCommand>.Default);
            services.AddScoped<IValidator<IDeletePatientByIdCommand>>(x => Validator<IDeletePatientByIdCommand>.Default);
            services.AddScoped<IValidator<IWantOneResource<Guid, Guid, PatientInfo>>>(x => Validator<IWantOneResource<Guid, Guid, PatientInfo>>.Default);
            services.AddScoped<IValidator<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>>(x => Validator<IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo>>.Default);
            services.AddScoped<IValidator<IPatchCommand<Guid, PatientInfo>>, PatchPatientCommandValidator>();
            services.AddScoped<IValidator<ICreateDocumentForPatientCommand>>(x => Validator<ICreateDocumentForPatientCommand>.Default);


            services.AddScoped<IHandleGetOnePatientInfoByIdQuery, HandleGetPatientInfoByIdQuery>();
            services.AddScoped<IHandleGetPageOfPatientInfosQuery, HandleGetPageOfPatientInfoQuery>();
            services.AddScoped<IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo>, HandleGetOnePhysiologicalMeasurementInfoQuery<Temperature, TemperatureInfo>>();
            services.AddScoped<IHandleGetMostRecentPhysiologicalMeasuresQuery<TemperatureInfo>, HandleGetMostRecentPhysiologicalMeasuresQuery<Temperature, TemperatureInfo>>();
            services.AddScoped<IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo>, HandleGetOnePhysiologicalMeasurementInfoQuery<BloodPressure,BloodPressureInfo>>();
            services.AddScoped<IHandleGetMostRecentPhysiologicalMeasuresQuery<BloodPressureInfo>, HandleGetMostRecentPhysiologicalMeasuresQuery<BloodPressure,BloodPressureInfo>>();

            services.AddScoped<IRunCreatePatientCommand, RunCreatePatientCommand>();
            services.AddScoped<IRunDeletePatientByIdCommand, RunDeletePatientByIdCommand>();
            services.AddScoped<IRunPatchPatientCommand, RunPatchPatientCommand>();

            services.AddScoped<IPhysiologicalMeasureService, PhysiologicalMeasureService>();
            services.AddScoped<IPrescriptionService, PrescriptionService>();

            services.AddScoped<IRunCreateDocumentForPatientCommand, RunCreateDocumentForPatientCommand>();
            services.AddScoped<IHandleGetDocumentsByPatientIdQuery, HandleGetDocumentsByPatientIdQuery>();
            services.AddScoped<IHandleGetOneDocumentInfoByPatientIdAndDocumentId, HandleGetOneDocumentInfoByPatientIdAndDocumentidQuery>();
            
            
        }
    }
}
