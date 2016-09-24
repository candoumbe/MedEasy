using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Deeply clone the <paramref name="source"/> 
        /// </summary>
        /// <typeparam name="T">Type of the <paramref name="source"/></typeparam>
        /// <param name="source">The object to clone</param>
        /// <returns>A deep copy of the object</returns>
        public static T DeepClone<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            T clone = (T)(ReferenceEquals(source, null) 
                ? default(T) 
                : JsonConvert.DeserializeObject(JsonConvert.SerializeObject(source)));

            return clone;
        }

        /// <summary>
        /// This method is intend is to parse an object to extract its properties.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="IDictionary{TKey, TValue}"/></returns>

        public static IDictionary<string, object> ParseAnonymousObject(this object obj)
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();

            if (obj != null)
            { 
                dictionary = obj.GetType()
                    .GetRuntimeProperties()
                    .Where(pi => pi.CanRead && pi.GetValue(obj) != null)
                    .ToDictionary(pi => pi.Name, pi => pi.GetValue(obj));
            }
            return dictionary;
        }



        /// <summary>
        /// Converts an object to its string representation suitable for appending as query string in a url
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <returns>the query string representation preceeded with the "?" or an empty string</returns>

        public static string ToQueryString(this object obj) 
            => DictionaryExtensions.ToQueryString(ParseAnonymousObject(obj));

    }
}