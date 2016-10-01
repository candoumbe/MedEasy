﻿using MedEasy.Handlers.Queries;
using MedEasy.Queries.Autocomplete;
using System;
using System.Collections.Generic;

namespace MedEasy.Handlers.Autocomplete
{
    public interface IHandleAutocompleteCityNameQuery: IHandleQueryAsync<Guid, string, IEnumerable<string>, IWantAutocompleteCityNameQuery>
    {

    }

}