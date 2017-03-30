using MedEasy.DTO;
using System;

namespace MedEasy.Commands.Patient
{

    public interface IDeleteOnePhysiologicalMeasureCommand<TKey, TData> : ICommand<TKey, TData>
        where TKey : IEquatable<TKey>
        where TData : DeletePhysiologicalMeasureInfo
    {
        string ToString();
    }
}