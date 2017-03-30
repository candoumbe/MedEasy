using MedEasy.DTO;
using MedEasy.Objects;
using System;

namespace MedEasy.Commands.Patient
{

    public interface IAddNewPhysiologicalMeasureCommand<TKey, TData> : ICommand<TKey, CreatePhysiologicalMeasureInfo<TData>>
        where TKey : IEquatable<TKey>
        where TData : PhysiologicalMeasurement
    {
        string ToString();
    }
}