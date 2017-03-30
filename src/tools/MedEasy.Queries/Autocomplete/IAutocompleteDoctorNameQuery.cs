using System;
using System.Collections.Generic;
using MedEasy.DTO.Autocomplete;

namespace MedEasy.Queries.Autocomplete
{
    public interface IAutocompleteDoctorNameQuery : IQuery<Guid, string, IEnumerable<DoctorAutocompleteInfo>>
    { 
    }
}