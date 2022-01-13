namespace Identity.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// <see cref="Account"/>'s identifier
    /// </summary>
    [JsonConverter(typeof(StronglyTypedIdJsonConverter<AccountId, Guid>))]
    public record AccountId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        /// <summary>
        /// Creates a new <see cref="AccountClaimId"/>
        /// </summary>
        /// <returns>The newly created <see cref="AccountClaimId"/></returns>
        public static AccountId New() => new(Guid.NewGuid());

        /// <summary>
        /// Creates a new <see cref="AccountId"/> value
        /// </summary>
        /// <returns>a new a unique <see cref="AccountId"/>.</returns>
        public static AccountId Empty => new(Guid.Empty);

        public override string ToString() => base.ToString();

        /// <summary>
        /// Handles EF conversion between <see cref="Guid"/> and <see cref="AccountId"/>
        /// </summary>
        public class EfValueConverter : ValueConverter<AccountId, Guid>
        {
            /// <summary>
            /// Builds a new <see cref="EfValueConverter"/> instance.
            /// </summary>
            /// <param name="mappingHints"></param>
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new AccountId(value),
                mappingHints
            )
            { }
        }
    }
}
