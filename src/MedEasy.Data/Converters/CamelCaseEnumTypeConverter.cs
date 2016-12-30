using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.Data.Converters
{
    /// <summary>
    /// Converter that specifies that enum
    /// </summary>
    public class CamelCaseEnumTypeConverter : StringEnumConverter
    {
        /// <summary>
        /// Builds a new <see cref="CamelCaseEnumTypeConverter"/> instance
        /// </summary>
        public CamelCaseEnumTypeConverter()
        {
            CamelCaseText = true;
        }

    }
}
