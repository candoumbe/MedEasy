using System;

namespace MedEasy.Ids
{
    public record StronglyTypedGuidId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
    }
}
