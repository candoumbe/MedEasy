using System;
using System.Collections.Generic;

namespace MedEasy.Queries.Autocomplete
{
    /// <summary>
    /// Interface for queries that requests list of city names given a string
    /// </summary>
    public interface IWantAutocompleteCityNameQuery : IWantManyResources<Guid, string, string>
    {
    }
}