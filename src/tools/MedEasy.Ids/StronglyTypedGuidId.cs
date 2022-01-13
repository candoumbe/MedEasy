namespace MedEasy.Ids
{
    using System;

    /// <summary>
    /// A strongly type that wrapped a <see cref="Guid"/> value.
    /// </summary>
    /// <param name="Value">The <see cref="Guid"/> value.</param>
    public record StronglyTypedGuidId(Guid Value) : StronglyTypedId<Guid>(Value);
}
