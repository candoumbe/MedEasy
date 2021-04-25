using MedEasy.Ids;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace Identity.Ids
{
    public record AccountClaimId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        /// <summary>
        /// Creates a new <see cref="AccountClaimId"/>
        /// </summary>
        /// <returns>The newly created <see cref="AccountClaimId"/></returns>
        public static AccountClaimId New() => new(Guid.NewGuid());

        public static AccountClaimId Empty => new(Guid.Empty);

        public class EfValueConverter : ValueConverter<AccountClaimId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new AccountClaimId(value),
                mappingHints
            )
            { }
        }
    }
}
