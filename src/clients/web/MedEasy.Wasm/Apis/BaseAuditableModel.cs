namespace MedEasy.Wasm.Apis
{
    using NodaTime;

    public record BaseAuditableModel<TKey>
    {
        public TKey Id { get; init; }

        public Instant CreatedDate { get; init; }

        public Instant UpdatedDate { get; init; }
    }
}
