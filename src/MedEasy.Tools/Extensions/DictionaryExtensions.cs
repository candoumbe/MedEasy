using System.Linq;
using System.Text;
using System.Reflection;
using static System.StringSplitOptions;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Converts a dictionary to its representation
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static string ToQueryString(this IDictionary<string, object> dictionary)
        {


            StringBuilder sb = new StringBuilder();
            IEnumerable<KeyValuePair<string, object>> keysAndValues = dictionary?
                .Where(kv => kv.Value != null) ?? Enumerable.Empty<KeyValuePair<string, object>>();
            IEnumerable<string> keys = dictionary.Keys;
            foreach (var key in keys)
            {
                object value = dictionary[key];
                TypeInfo valueType = value.GetType().GetTypeInfo();
                //The type of the value is a complex object
                if (!(valueType.IsPrimitive || valueType.BaseType == typeof(string)))
                {
                    dictionary.Remove(key);
                    string[] localQueryStringParts = value.ToQueryString().Split(new[] { '&' }, RemoveEmptyEntries);

                    foreach (string queryStringPart in localQueryStringParts)
                    {
                        string[] queryParts = queryStringPart.Split(new[] { '=' }, RemoveEmptyEntries);
                        if (queryParts.Length == 2)
                        {
                            dictionary.Add($"{key}.{queryParts[0]}", queryParts[1]);
                        }
                    }
                }
                else if (valueType.BaseType == typeof(IEnumerable))
                {
                    IEnumerable enumerable = dictionary[key] as IEnumerable;
                    dictionary.Remove(key);
                    int nbElementsLu = 0;
                    foreach (object item in enumerable)
                    {
                        if (item != null)
                        {
                            dictionary[$"{key}[{nbElementsLu}]"] = item.ToString();
                            nbElementsLu++;
                        }
                    }

                }

            }

            string queryString = string.Join("&", dictionary.Select(x => $"{Uri.EscapeUriString(x.Key)}={Uri.EscapeUriString(x.Value.ToString())}"));

            return queryString;
        }
    }
}