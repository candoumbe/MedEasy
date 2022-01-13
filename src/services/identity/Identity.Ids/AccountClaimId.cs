namespace Identity.Ids
{
    using MedEasy.Ids;
    using MedEasy.Ids.Converters;

    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Wraps the identifier of an AccountClaim.
    /// </summary>
    /// <param name="Value"></param>
    [JsonConverter(typeof(StronglyTypedIdJsonConverter<AccountClaimId, Guid>))]
    public record AccountClaimId(Guid Value) : StronglyTypedId<Guid>(Value)
    {
        /// <summary>
        /// Creates a new <see cref="AccountClaimId"/>
        /// </summary>
        /// <returns>The newly created <see cref="AccountClaimId"/></returns>
        public static AccountClaimId New() => new(Guid.NewGuid());

        /// <summary>
        /// Value to use for whenever a value for <see cref="AccountClaimId"/> is needed and none can be provided.
        /// </summary>
        public static AccountClaimId Empty => new(Guid.Empty);

        ///<inheritdoc/>
        public override string ToString() => base.ToString();

        /// <summary>
        ///Converter <see cref="Guid"/>  <see cref="AccountClaimId"/> value converter.
        /// </summary>
        public class EfValueConverter : ValueConverter<AccountClaimId, Guid>
        {
            /// <summary>
            /// Builds a new <see cref="EfValueConverter"/>
            /// </summary>
            /// <param name="mappingHints"></param>
            public EfValueConverter(ConverterMappingHints mappingHints = null) : base(
                id => id.Value,
                value => new AccountClaimId(value),
                mappingHints
            )
            { }
        }
    }
}
