using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Queries;

namespace MedEasy.Services
{
    /// <summary>
    /// Handles everything related to <see cref="PhysiologicalMeasurementInfo"/> resource
    /// </summary>
    public interface IPhysiologicalMeasureService
    {
        /// <summary>
        /// Asynchronously gets the most recent <see cref="BloodPressureInfo"/> measures.
        /// </summary>
        /// <param name="query">specifies which patient to get its most recent measures for</param>
        /// <returns><see cref="IEnumerable{T}"/>holding the most recent <see cref="BloodPressureInfo"/></returns>
        Task<IEnumerable<BloodPressureInfo>> MostRecentBloodPressuresAsync(IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<BloodPressureInfo>> query);

        /// <summary>
        /// Gets the most recent <see cref="TemperatureInfo"/> measures.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="query">specifies which patient to get its most recent measures for</param>
        /// <returns><see cref="IEnumerable{T}"/>holding the most recent <see cref="TemperatureInfo"/></returns>
        /// <see cref="GetMostRecentPhysiologicalMeasuresInfo"/>
        Task<IEnumerable<TemperatureInfo>> MostRecentTemperaturesAsync(IQuery<Guid, GetMostRecentPhysiologicalMeasuresInfo, IEnumerable<TemperatureInfo>> query);

        /// <summary>
        /// Gets the 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<TemperatureInfo> GetOneTemperatureMeasureAsync(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TemperatureInfo> query);

        /// <summary>
        /// Add a new <see cref="BloodPressureInfo"/>
        /// </summary>
        /// <param name="query">command that holds data to create the resource</param>
        /// <returns>The created resource</returns>
        Task<BloodPressureInfo> AddNewBloodPressureMeasureAsync(IAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo> query);

        /// <summary>
        /// Add a new <see cref="TemperatureInfo"/>
        /// </summary>
        /// <param name="query">command that holds data to create the resource</param>
        /// <returns>The created resource</returns>
        Task<TemperatureInfo> AddNewTemperatureMeasureAsync(IAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo> query);

        /// <summary>
        /// Gets the <see cref="BloodPressureInfo"/>.
        /// </summary>
        /// <param name="query">the query to get the resource</param>
        /// <returns></returns>
        Task<BloodPressureInfo> GetOneBloodPressureInfoAsync(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, BloodPressureInfo> query);

        /// <summary>
        /// Gets the <see cref="BodyWeightInfo"/>
        /// </summary>
        /// <param name="query">the query to get the resource</param>
        /// <returns>the resource or <c>null</c>if there's no patient info or the resource doesn't exist</returns>
        Task<BodyWeightInfo> GetOneBodyWeightInfoAsync(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, BodyWeightInfo> query);
    }
}
