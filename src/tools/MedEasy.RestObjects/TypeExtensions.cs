using MedEasy.RestObjects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using static System.Collections.Generic.DictionaryExtensions;

namespace System
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Builds a <see cref="Form"/> out of a <see cref="Type"/>
        /// </summary>
        /// <param name="t">Type for which a <see cref="Form"/> representation will be build.</param>
        /// <param name="linkToForm"><see cref="Link"/> where to submit data described by the resulting <see cref="Form"/></param>
        /// <returns><see cref="Form"/> representation of <paramref name="t"/></returns>
        public static Form ToForm(this Type t, Link linkToForm)
        {
            Form f = new() { Meta = linkToForm };

            IEnumerable<PropertyInfo> properties = t.GetRuntimeProperties()
                .Where(x => x.CanRead && PrimitiveTypes.Contains(x.PropertyType))
                .ToList();

            IList<FormField> fields = new List<FormField>(properties.Count());
            foreach (PropertyInfo pi in properties)
            {
                FormField ff = new() { Name = pi.Name };

                IEnumerable<Attribute> attributes = pi.GetCustomAttributes(inherit: true)
#if !NETSTANDARD1_1
                    .Cast<Attribute>()
#endif
                    ;
                DisplayAttribute displayAttribute = (DisplayAttribute)attributes
                    .FirstOrDefault(x => typeof(DisplayAttribute).Equals(x.GetType()));

                ff.Label = displayAttribute?.GetName() ?? pi.Name;
                ff.Placeholder = displayAttribute?.GetPrompt();

                DataTypeAttribute dataTypeAttribute = (DataTypeAttribute)attributes
                        .FirstOrDefault(x => typeof(DataTypeAttribute).Equals(x.GetType()));

                if (pi.PropertyType.Equals(typeof(string)))
                {
                    ff.Type = FormFieldType.String;
                    StringLengthAttribute stringLengthAttribute = (StringLengthAttribute)attributes
                        .FirstOrDefault(x => typeof(StringLengthAttribute).Equals(x.GetType()));

                    ff.MaxLength = stringLengthAttribute?.MaximumLength;
                    ff.MinLength = stringLengthAttribute?.MinimumLength;

                    ff.Secret = dataTypeAttribute != null && dataTypeAttribute.DataType == DataType.Password
                        ? true
                        : (bool?)null;
                }
                else if (pi.PropertyType.Equals(typeof(DateTime)) || pi.PropertyType.Equals(typeof(DateTime?))
                    || pi.PropertyType.Equals(typeof(DateTimeOffset?)) || pi.PropertyType.Equals(typeof(DateTimeOffset)))
                {
                    ff.Type = dataTypeAttribute?.DataType == DataType.DateTime
                        ? FormFieldType.DateTime
                        : FormFieldType.Date;
                    if (!pi.PropertyType.IsAssignableToGenericType(typeof(Nullable<>)))
                    {
                        ff.Required = true;
                    }
                }
                else if (NumericTypes.Contains(pi.PropertyType))
                {
                    ff.Type = FormFieldType.Integer;
                    ff.Min = 0;
                }

                if (attributes.Any(x => typeof(RequiredAttribute).Equals(x.GetType())))
                {
                    ff.Required = true;
                }
                fields.Add(ff);
            }

            f.Items = fields;

            return f;
        }
    }
}
