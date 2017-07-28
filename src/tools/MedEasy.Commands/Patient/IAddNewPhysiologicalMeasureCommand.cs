using MedEasy.DTO;
using MedEasy.Objects;
using System;

namespace MedEasy.Commands.Patient
{

    public interface IAddNewPhysiologicalMeasureCommand<TKey, TData, TOutput> : ICommand<TKey, CreatePhysiologicalMeasureInfo<TData>, TOutput>
        where TKey : IEquatable<TKey>
        where TData : PhysiologicalMeasurement
    {
        string ToString();
    }
}