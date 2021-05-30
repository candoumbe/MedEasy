namespace MedEasy.Ids
{
    using System;

    public record StronglyTypedGuidId(Guid Value) : StronglyTypedId<Guid>(Value);
}
