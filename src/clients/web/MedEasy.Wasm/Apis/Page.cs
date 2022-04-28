namespace MedEasy.Wasm.Apis
{
    using MedEasy.RestObjects;

    /// <summary>
    /// Wraps a result set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Page<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        public int TotalCount { get; set; }

        public PageLinks Links { get; set; }
    }
}
