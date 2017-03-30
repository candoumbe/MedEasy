using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries.Autocomplete;
using System;
using System.Collections.Generic;

namespace MedEasy.Handlers.Core.Autocomplete.Queries
{
    public interface IHandleAutocompleteCityNameQuery: IHandleQueryAsync<Guid, string, IEnumerable<string>, IWantAutocompleteCityNameQuery>
    {

    }

}
