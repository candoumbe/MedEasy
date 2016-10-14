using System;
using MedEasy.Commands.Patient;
using MedEasy.Handlers.Commands;
using MedEasy.DTO;

namespace MedEasy.Handlers.Patient.Commands
{

    /// <summary>
    /// Defines the method that classes that can delete one <see cref="PhysiologicalMeasurementInfo"/> at a time.
    /// </summary>
    /// <remarks>
    /// The deletion of the <see cref="PhysiologicalMeasurementInfo"/> is done usin
    /// </remarks>
    public interface IRunDeletePhysiologicalMeasureCommand<TKey, TData> : IRunCommandAsync<TKey, TData, IDeleteOnePhysiologicalMeasureCommand<TKey, TData>>
        where TKey : IEquatable<TKey>
        where TData : DeletePhysiologicalMeasureInfo
       
    {
        
    }
}
