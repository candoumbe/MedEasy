using System;

namespace MedEasy.Queries.Autocomplete
{
    /// <summary>
    /// Base class for autocomplete queries
    /// </summary>
    /// <typeparam name="TKey">Type of the key that uniquely identifies a query</typeparam>
    /// <typeparam name="TResult">Type of the suggestion result</typeparam>
    public abstract class AutocompleteQueryBase<TKey, TResult> : IQuery<TKey, string, TResult>
        where TKey : IEquatable<TKey>
    {
        public TKey Id { get; }

        public string Data { get; }

        protected AutocompleteQueryBase(TKey id, string term)
        {
            if (term == null)
            {
                throw new ArgumentNullException(nameof(term));
            }
            Id = id;
            Data = term.Trim();

        }


        public override string ToString() => $"Autocomplete - {nameof(Id)}:{Id}, {nameof(Data)}:{Data}";
    }
}
