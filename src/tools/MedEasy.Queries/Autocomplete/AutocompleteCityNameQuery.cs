using System;
using System.Collections.Generic;

namespace MedEasy.Queries.Autocomplete
{
    /// <summary>
    /// An instance of this class represents a query to get the list of birthplace
    /// </summary>
    public class AutocompleteCityNameQuery : AutocompleteQueryBase<Guid, IEnumerable<string>>, IWantAutocompleteCityNameQuery
    {
        /// <summary>
        /// Builds a new instance of the query
        /// </summary>
        /// <param name="options"></param>
        public AutocompleteCityNameQuery(string term) : base(Guid.NewGuid(), term)
        {
            
        }
    }
}
