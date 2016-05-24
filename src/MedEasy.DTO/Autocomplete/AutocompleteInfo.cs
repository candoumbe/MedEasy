using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO.Autocomplete
{
    public abstract class AutocompleteInfo<TValue>
    {
        public TValue Value { get; set; }

        public string Text { get; set; }

    }
}
