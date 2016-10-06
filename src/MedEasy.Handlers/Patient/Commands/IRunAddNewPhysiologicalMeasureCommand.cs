using System;
using MedEasy.Commands.Patient;
using MedEasy.Handlers.Commands;
using MedEasy.DTO;

namespace MedEasy.Handlers.Patient.Commands
{

    /// <summary>
    /// An instance of this class process <see cref="IRunAddNewPhysiologicalMeasureCommand{TData, TOutput}"/> commands
    /// </summary>
    public interface IRunAddNewPhysiologicalMeasureCommand<TKey, TData, TOutput> : IRunCommandAsync<TKey, TData, TOutput, IAddNewPhysiologicalMeasureCommand<TKey, TData>>
        where TKey : IEquatable<TKey>
        where TData : CreatePhysiologicalMeasureInfo
       
    {
        
    }
}
