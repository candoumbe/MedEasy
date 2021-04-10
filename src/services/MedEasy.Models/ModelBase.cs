using MedEasy.RestObjects;

using System;

namespace MedEasy.Models
{
    public class ModelBase<T> : Resource<T>
        where T : IEquatable<T>
    {
    }
}
