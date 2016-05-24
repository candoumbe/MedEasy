using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace System
{
    public static class ObjectExtensions
    {

        public static T DeepClone<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            T clone = (T)(ReferenceEquals(source, null) 
                ? default(T) 
                : JsonConvert.DeserializeObject(JsonConvert.SerializeObject(source)));

            return clone;
        }
    }
}
