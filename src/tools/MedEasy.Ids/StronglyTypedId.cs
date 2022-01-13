namespace MedEasy.Ids
{
    using MedEasy.Ids.Converters;

    using System.ComponentModel;

    /// <summary>
    /// Base class for storngly typed ids.
    /// </summary>
    /// <typeparam name="TValue">Type fo the underlying id.</typeparam>
    [TypeConverter(typeof(StronglyTypedIdTypeConverter))]
#if NET5_0_OR_GREATER
    public abstract record StronglyTypedId<TValue>(TValue Value)
        where TValue : notnull
    {
        ///<inheritdoc/>
        public override string ToString() => Value.ToString();
    }
#else
    public abstract record StronglyTypedId<TValue>
        where TValue : notnull
    {
        /// <summary>
        /// Raw value wrapped inside the current instance
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// Builds a new <see cref="StronglyTypedId{TValue}"/> instance.
        /// </summary>
        /// <param name="value"></param>
        protected StronglyTypedId(TValue value) => Value = value;

        ///<inheritdoc/>
        public override string ToString() => Value.ToString();
    }
#endif
}
