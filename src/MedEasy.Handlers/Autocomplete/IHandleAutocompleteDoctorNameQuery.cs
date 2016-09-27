using MedEasy.DTO.Autocomplete;
using MedEasy.Handlers.Queries;
using MedEasy.Queries.Autocomplete;
using System;
using System.Collections.Generic;

namespace MedEasy.Handlers.Autocomplete
{
    public interface IHandleAutocompleteDoctorNameQuery: IHandleQueryAsync<Guid, string, IEnumerable<DoctorAutocompleteInfo>, IAutocompleteDoctorNameQuery>
    {

    }

}
