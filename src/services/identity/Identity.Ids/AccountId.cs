using MedEasy.Ids;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using System;

namespace Identity.Ids
{
    /// <summary>
    /// <see cref="Account"/>'s identifier
    /// </summary>
    public record AccountId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        /// <summary>
        /// Creates a new <see cref="AccountClaimId"/>
        /// </summary>
        /// <returns>The newly created <see cref="AccountClaimId"/></returns>
        public static AccountId New() => new(Guid.NewGuid());

        public static AccountId Empty => new(Guid.Empty);


        public class EfValueConverter : ValueConverter<AccountId, Guid>
        {
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new AccountId(value),
                mappingHints
            )
            { }
        }
    }

}
