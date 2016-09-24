using System;
using System.Collections.Generic;

namespace MedEasy.Queries.Autocomplete
{
    /// <summary>
    /// An instance of this class represents a query to get the list doctors name that contains a specific term.
    /// 
    /// </summary>
    public class AutocompleteDoctorNameQuery : AutocompleteQueryBase<Guid, IEnumerable<string>>, IAutocompleteDoctorNameQuery
    {
        /// <summary>
        /// Builds a new instance of the query
        /// </summary>
        /// <param name="term"></param>
        public AutocompleteDoctorNameQuery(string term) : base(Guid.NewGuid(), term)
        {           
        }
    }
}
