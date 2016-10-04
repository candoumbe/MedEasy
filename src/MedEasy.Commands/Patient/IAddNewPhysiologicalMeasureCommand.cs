using MedEasy.DTO;
using System;

namespace MedEasy.Commands.Patient
{

    public interface IAddNewPhysiologicalMeasureCommand<TKey, TData> : ICommand<TKey, TData>
        where TKey : IEquatable<TKey>
        where TData : CreatePhysiologicalMeasureInfo
    {
        string ToString();
    }
}