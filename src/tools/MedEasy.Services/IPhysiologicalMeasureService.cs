using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedEasy.Commands.Patient;
using MedEasy.DTO;
using MedEasy.Queries;
using MedEasy.Objects;
using MedEasy.Commands;
using System.Threading;
using Optional;
using MedEasy.Queries.Patient;
using MedEasy.Handlers.Core.Exceptions;

namespace MedEasy.Services
{
    /// <summary>
    /// Handles everything related to <see cref="PhysiologicalMeasurementInfo"/> resource.
    /// </summary>
    public interface IPhysiologicalMeasureService
    {
        /// <summary>
        /// Asynchronously gets the most recent <typeparamref name="TPhysiologicalMeasureInfo"/> measures.
        /// </summary>
        /// <param name="query">specifies which patient to get its most recent measures for</param>
        /// <returns><see cref="IEnumerable{T}"/>holding the most recent <see cref="TPhysiologicalMeasureInfo"/></returns>
        ValueTask<Option<IEnumerable<TPhysiologicalMeasureInfo>>> GetMostRecentMeasuresAsync<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(IWantMostRecentPhysiologicalMeasuresQuery<TPhysiologicalMeasureInfo> query, CancellationToken cancellationToken = default(CancellationToken))
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            where TPhysiologicalMeasureInfo : PhysiologicalMeasurementInfo;
                
        /// <summary>
        /// Add a new <typeparamref name="TPhysiologicalMeasure"/>
        /// </summary>
        /// <param name="command">command that holds data to create the resource</param>
        /// <returns>The created resource</returns>
        ValueTask<Option<TPhysiologicalMeasureInfo, CommandException>> AddNewMeasureAsync<TPhysiologicalMeasure, TPhysiologicalMeasureInfo>(ICommand<Guid, CreatePhysiologicalMeasureInfo<TPhysiologicalMeasure>> query, CancellationToken cancellationToken = default(CancellationToken))
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            where TPhysiologicalMeasureInfo : PhysiologicalMeasurementInfo;
        
        /// <summary>
        /// Gets the <see cref="TPhysiologicalMesureInfo"/>
        /// </summary>
        /// <param name="query">the query to get the resource</param>
        /// <returns>the resource or <c>null</c>if there's no patient info or the resource doesn't exist</returns>
        ValueTask<Option<TPhysiologicalMesureInfo>> GetOneMeasureAsync<TPhysiologicalMeasure, TPhysiologicalMesureInfo>(IWantOneResource<Guid, GetOnePhysiologicalMeasureInfo, TPhysiologicalMesureInfo> query, CancellationToken cancellationToken = default(CancellationToken))
            where TPhysiologicalMeasure : PhysiologicalMeasurement
            where TPhysiologicalMesureInfo : PhysiologicalMeasurementInfo;

        /// <summary>
        /// Deletes one <see cref="PhysiologicalMeasurement"/> resource.
        /// </summary>
        /// <param name="command">specifies the resource to delete</param>
        /// <typeparam name="TPhysiologicalMeasure">Type of the resource to delete</typeparam>
        /// <exception cref="CommandNotValidException{TCommandId}">if <paramref name="command"/> is not valid</exception>
        Task DeleteOnePhysiologicalMeasureAsync<TPhysiologicalMeasure>(IDeleteOnePhysiologicalMeasureCommand<Guid, DeletePhysiologicalMeasureInfo> command, CancellationToken cancellationToken = default(CancellationToken)) 
            where TPhysiologicalMeasure : PhysiologicalMeasurement;
    }
}
