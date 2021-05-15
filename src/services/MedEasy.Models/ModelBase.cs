namespace MedEasy.Models
{
    using MedEasy.RestObjects;

    using System;

    public class ModelBase<T> : Resource<T>
        where T : IEquatable<T>
    {
    }
}
