using System.Linq;
using System.Text;
using System.Reflection;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// List of all types that can be directly converted to their string representation
        /// </summary>
        public static IEnumerable<Type> PrimitiveTypes = new[]
        {
            typeof(string),

            typeof(int), typeof(int?),
            typeof(long), typeof(long?),
            typeof(short), typeof(short?),
            typeof(decimal), typeof(decimal?),
            typeof(float), typeof(float?),

            typeof(DateTime), typeof(DateTime?),
            typeof(DateTimeOffset), typeof(DateTimeOffset?),
            typeof(Guid), typeof(Guid?),
            typeof(bool), typeof(bool?)


        };


        public static IEnumerable<Type> NumericTypes = new[]
        {
            typeof(int), typeof(int?),
            typeof(long), typeof(long?),
            typeof(short), typeof(short?),
            typeof(decimal), typeof(decimal?),
            typeof(float), typeof(float?),
        };



        /// <summary>
        /// Converter for a datetime
        /// </summary>
        private static Func<DateTime, string> _fnDateTimeToQueryString = x => x.ToString("x");

        /// <summary>
        /// Converts a dictionary to a "URL" friendly representation
        /// </summary>
        /// <param name="dictionary">the dictionary to convert</param>
        /// <returns></returns>
        public static string ToQueryString(this IDictionary<string, object> dictionary)
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, object> kv in dictionary.Where(kv => kv.Value != null))
            {
                object value = kv.Value;
                Type valueType = value.GetType();
                TypeInfo valueTypeInfo = valueType.GetTypeInfo();
                //The type of the value is a "simple" object
                if (valueTypeInfo.IsPrimitive || valueTypeInfo.IsEnum || PrimitiveTypes.Any(x =>  x == valueType) )
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("&");
                    }

                    sb
                        .Append(Uri.EscapeDataString(kv.Key))
                        .Append("=");

                    // DateTime/DateTimeOffset should be encoded in ISO format
                    switch (value)
                    {
                        case DateTime date:
                            sb.Append(date.ToString("s"));
                            break;
                        case DateTimeOffset date:
                            sb.Append(date.ToString("s"));
                            break;
                        default:
                            sb.Append(Uri.EscapeDataString(kv.Value.ToString()));
                            break;
                    }
                }
                else if (value is IDictionary<string, object>)
                {
                    IDictionary<string, object> subDictionary = ((IDictionary<string, object>)value)
#if !NETSTANDARD1_0 && !NETSTANDARD1_3
                                    .AsParallel()
#endif
                                    .ToDictionary(x => $"{kv.Key}[{x.Key}]", x => x.Value);

                    if (sb.Length > 0)
                    {
                        sb.Append("&");
                    }
                    sb.Append(ToQueryString(subDictionary));
                }
                else if (valueTypeInfo.BaseType == typeof(IEnumerable))
                {
                    IEnumerable enumerable = kv.Value as IEnumerable;
                    int itemPosition = 0;
                    Type elementType;
                    TypeInfo elementTypeInfo;
                    foreach (object item in enumerable)
                    {
                        if (item != null)
                        {
                            elementType = item.GetType();
                            elementTypeInfo = elementType.GetTypeInfo();
                            if (elementTypeInfo.IsPrimitive || PrimitiveTypes.Any(x => x == elementType))
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append("&");
                                }
                                sb.Append($"{Uri.EscapeDataString($"{kv.Key}[{itemPosition}]")}")
                                   .Append("=")
                                   .Append(Uri.EscapeDataString(kv.Value.ToString()));

                                itemPosition++;
                            }
                        }
                    }
                }
            }

            
            return sb.ToString();
        }
    }
}