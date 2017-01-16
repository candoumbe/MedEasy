using MedEasy.DTO.Autocomplete;
using MedEasy.Handlers.Core.Queries;
using MedEasy.Queries.Autocomplete;
using System;
using System.Collections.Generic;

namespace MedEasy.Handlers.Core.Autocomplete
{
    public interface IHandleAutocompleteDoctorNameQuery: IHandleQueryAsync<Guid, string, IEnumerable<DoctorAutocompleteInfo>, IAutocompleteDoctorNameQuery>
    {

    }

}
