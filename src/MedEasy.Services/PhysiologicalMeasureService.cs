﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Queries;

namespace MedEasy.Services
{
    /// <summary>
    /// Handles everything related to <see cref="PhysiologicalMeasurementInfo"/>
    /// </summary>
    public class PhysiologicalMeasureService : IPhysiologicalMeasureService
    {
        private readonly IRunAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo, TemperatureInfo> _iRunAddNewTemperatureCommand;
        private readonly IRunAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo, BloodPressureInfo> _iRunAddNewBloodPressureCommand;
        private readonly IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo> _iHandleGetOneTemperatureQuery;
        private readonly IHandleGetOnePhysiologicalMeasureQuery<BodyWeightInfo> _iHandleGetOneBodyWeightQuery;
        private readonly IHandleGetMostRecentPhysiologicalMeasuresQuery<BloodPressureInfo> _iHandleGetMostRecentBloodPressureMeasureQuery;
        private readonly IHandleGetMostRecentPhysiologicalMeasuresQuery<TemperatureInfo> _iHandleGetMostRecentTemperatureMeasuresQuery;
        private readonly IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo> _iHandleGetOneBloodPressureQuery;
        private readonly IRunDeletePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo, BloodPressureInfo> _iRunDeleteBloodPressureCommand;
        private readonly IRunDeletePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo, TemperatureInfo> _iRunDeleteTemperatureCommand;

        /// <summary>
        /// Builds a new <see cref="PhysiologicalMeasureService"/> instance
        /// </summary>
        /// <param name="iRunAddNewTemperatureCommand">instance that can create <see cref="TemperatureInfo"/></param>
        /// <param name="iRunAddNewBloodPressureCommand">instance that can create <see cref="BloodPressureInfo"/></param>
        /// <param name="iHandleGetOneTemperatureQuery">instance that can retrieve one <see cref="TemperatureInfo"/></param>
        /// <param name="iHandleGetOneBloodPressureQuery">instance that can retrieve one <see cref="BloodPressureInfo"/></param>
        /// <param name="iHandleGetMostRecentBloodPressureMeasureQuery">instance that can retrieve most recent <see cref="BloodPressureInfo"/> measures.</param>
        /// <param name="iHandleGetMostRecentTemperatureMeasuresQuery">instance that can retrieve most recent <see cref="TemperatureInfo"/> measures.</param>
        public PhysiologicalMeasureService(
            IRunAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo, TemperatureInfo> iRunAddNewTemperatureCommand, 
            IRunAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo, BloodPressureInfo> iRunAddNewBloodPressureCommand, 
            IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo> iHandleGetOneTemperatureQuery,
            IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo> iHandleGetOneBloodPressureQuery, 
            IHandleGetMostRecentPhysiologicalMeasuresQuery<BloodPressureInfo> iHandleGetMostRecentBloodPressureMeasureQuery, 
            IHandleGetMostRecentPhysiologicalMeasuresQuery<TemperatureInfo> iHandleGetMostRecentTemperatureMeasuresQuery,
            IRunDeletePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo, BloodPressureInfo> iRunDeleteBloodPressureCommand,
            IRunDeletePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo, TemperatureInfo> iRunDeleteTemperatureCommand
            )
        {
            _iRunAddNewTemperatureCommand = iRunAddNewTemperatureCommand;
            _iRunAddNewBloodPressureCommand = iRunAddNewBloodPressureCommand;
            _iHandleGetOneTemperatureQuery = iHandleGetOneTemperatureQuery;
            _iHandleGetOneBloodPressureQuery = iHandleGetOneBloodPressureQuery;
            _iHandleGetMostRecentBloodPressureMeasureQuery = iHandleGetMostRecentBloodPressureMeasureQuery;
            _iHandleGetMostRecentTemperatureMeasuresQuery = iHandleGetMostRecentTemperatureMeasuresQuery;

            _iRunDeleteBloodPressureCommand = iRunDeleteBloodPressureCommand;
            _iRunDeleteTemperatureCommand = iRunDeleteTemperatureCommand;
        }

        /// <summary>
        /// Asynchronously gets the most recent <see cref="BloodPressureInfo"/> measures.
        /// </summary>
        /// <param name="query">specifies which patient to get its most recent measures for</param>
        /// <returns><see cref="IEnumerable{T}"/>holding the most recent <see cref="BloodPressureInfo"/></returns>
        public async Task<IEnumerable<BloodPressureInfo>> MostRecentBloodPressuresAsync(IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<BloodPressureInfo>> query)
            => await _iHandleGetMostRecentBloodPressureMeasureQuery.HandleAsync(query).ConfigureAwait(false);
        /// <summary>
        /// Gets the most recent <see cref="TemperatureInfo"/> measures.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="query">specifies which patient to get its most recent measures for</param>
        /// <returns><see cref="IEnumerable{T}"/>holding the most recent <see cref="TemperatureInfo"/></returns>
        /// <see cref="GetMostRecentPhysiologicalMeasuresInfo"/>
        public async Task<IEnumerable<TemperatureInfo>> MostRecentTemperaturesAsync(IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TemperatureInfo>> query)
            => await _iHandleGetMostRecentTemperatureMeasuresQuery.HandleAsync(query).ConfigureAwait(false);

        /// <summary>
        /// Gets the <see cref="TemperatureInfo"/> resource
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<TemperatureInfo> GetOneTemperatureMeasureAsync(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo> query)
            => await _iHandleGetOneTemperatureQuery.HandleAsync(query).ConfigureAwait(false);

        /// <summary>
        /// Add a new <see cref="BloodPressureInfo"/>
        /// </summary>
        /// <param name="command">command that holds data to create the resource</param>
        /// <returns>The created resource</returns>
        public async Task<BloodPressureInfo> AddNewBloodPressureMeasureAsync(IAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo> command)
            => await _iRunAddNewBloodPressureCommand.RunAsync(command).ConfigureAwait(false);

        /// <summary>
        /// Add a new <see cref="TemperatureInfo"/>
        /// </summary>
        /// <param name="addNewPhysiologicalMeasureCommand">command that holds data to create the resource</param>
        /// <returns>The created resource</returns>
        public async Task<TemperatureInfo> AddNewTemperatureMeasureAsync(IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo> addNewPhysiologicalMeasureCommand)
            => await _iRunAddNewTemperatureCommand.RunAsync(addNewPhysiologicalMeasureCommand).ConfigureAwait(false);

        /// <summary>
        /// Gets the <see cref="BloodPressureInfo"/>.
        /// </summary>
        /// <param name="wantOneResource">the query to get the resource</param>
        /// <returns></returns>
        public async Task<BloodPressureInfo> GetOneBloodPressureInfoAsync(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, BloodPressureInfo> wantOneResource)
            => await _iHandleGetOneBloodPressureQuery.HandleAsync(wantOneResource).ConfigureAwait(false);


        /// <summary>
        /// Gets the <see cref="BodyWeightInfo"/>.
        /// </summary>
        /// <param name="query">the query to get the resource</param>
        /// <returns>The resource found or <c>null</c> if no resource</returns>
        public async Task<BodyWeightInfo> GetOneBodyWeightInfoAsync(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, BodyWeightInfo> query)
            => await _iHandleGetOneBodyWeightQuery.HandleAsync(query).ConfigureAwait(false);

        /// <summary>
        /// Deletes the resource 
        /// </summary>
        /// <param name="command"></param>
        public async Task DeleteOneBloodPressureAsync(IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo> command)
        {
            await _iRunDeleteBloodPressureCommand.RunAsync(command);
        }
    }
}
