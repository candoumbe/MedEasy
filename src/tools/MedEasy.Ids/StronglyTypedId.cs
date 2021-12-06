namespace MedEasy.Ids
{
    using MedEasy.Ids.Converters;

    using System.ComponentModel;

    /// <summary>
    /// Base class for storngly typed ids.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [TypeConverter(typeof(StronglyTypedIdTypeConverter))]
#if NET5_0
    public abstract record StronglyTypedId<TValue>(TValue Value)
        where TValue : notnull
    {
        public override string ToString() => Value.ToString();
    }
#else
    public abstract record StronglyTypedId<TValue>
        where TValue : notnull
    {
        public TValue Value { get; }

        protected StronglyTypedId(TValue value) => Value = value;

        public override string ToString() => Value.ToString();
    }
#endif
}
