using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Optional;

using static MedEasy.RestObjects.FormFieldType;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// Helper class to build a <see cref="Form"/> instance
    /// </summary>
    /// <typeparam name="T">Type to build a <see cref="Form"/> for.</typeparam>
    public class FormBuilder<T>
    {
        private static readonly HashSet<Type> _dateTypes = new()
        {
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
        };

        private static readonly HashSet<Type> _numericTypes = new()
        {
            typeof(int),
            typeof(int?),
            typeof(float),
            typeof(float?),
            typeof(long),
            typeof(long?),
            typeof(double),
            typeof(double?),
            typeof(short),
            typeof(short?),
            typeof(decimal),
            typeof(decimal?),

        };

        private readonly IList<FormField> _fields;
        private readonly Link _meta;

        /// <summary>
        /// Creates a new <see cref="FormBuilder{T}"/> instance
        /// </summary>
        /// <param name="meta">describes where and how to send the form's data</param>
        public FormBuilder(Link meta = null)
        {
            _fields = new List<FormField>();
            _meta = meta;
        }

        /// <summary>
        /// Adds a field to the <see cref="Form"/>'s configuration.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        /// <param name="attributes">Overrides field's attributes</param>
        /// <returns></returns>
        public FormBuilder<T> AddField<TProperty>(Expression<Func<T, TProperty>> property, FormFieldAttributeOverrides attributes = null)
        {
            if (property.Body is MemberExpression me)
            {
                FormField field = new() { Name = me.Member.Name };
                Option<FormFieldAttributeOverrides> optionalAttributesOverride = attributes.SomeNotNull();
                Option<FormFieldAttribute> optionalFormFieldAttribute = me.Member.GetCustomAttribute<FormFieldAttribute>()
                    .SomeNotNull();

                if (_dateTypes.Contains(property.ReturnType))
                {
                    field.Type = FormFieldType.DateTime;
                }
                else if (_numericTypes.Contains(property.ReturnType))
                {
                    field.Type = Integer;
                }

                optionalFormFieldAttribute.MatchSome(
                    attr =>
                    {
                        if (attr.IsDescriptionSet)
                        {
                            field.Description = attr.Description;
                        }
                        if (attr.IsSecretSet)
                        {
                            field.Secret = attr.Secret;
                        }

                        if (attr.IsMinSet)
                        {
                            field.Min = attr.Min;
                        }

                        field.Pattern = attr.Pattern;
                        if (attr.IsTypeSet)
                        {
                            field.Type = attr.Type;
                        }
                        if (attr.IsMinSet)
                        {
                            field.Min = attr.Min;
                        }
                        if (attr.IsMaxLengthSet)
                        {
                            field.MaxLength = attr.MaxLength;
                        }
                        if (attr.IsTypeSet)
                        {
                            field.Type = attr.Type;
                        }
                    });

                optionalAttributesOverride.MatchSome((attrs) =>
                {
                    if (attrs.Min.HasValue)
                    {
                        field.Min = attrs.Min;
                    }
                    if (attrs.Secret.HasValue)
                    {
                        field.Secret = attrs.Secret;
                    }
                    if (attrs.IsDescriptionSet)
                    {
                        field.Description = attrs.Description;
                    }
                    field.Label = attrs.Label ?? field.Name;
                    if (attrs.Max.HasValue)
                    {
                        field.Max = attrs.Max;
                    }
                    if (attrs.Pattern != null)
                    {
                        field.Pattern = attrs.Pattern;
                    }
                    if (attrs.IsMaxLengthSet)
                    {
                        field.MaxLength = attrs.MaxLength;
                    }

                    if (attrs.IsTypeSet)
                    {
                        field.Type = attrs.Type;
                    }
                });


                field.Label = field.Name;
                field.Enabled = attributes?.Enabled;


                _fields.Add(field);
            }

            return this;
        }


        /// <summary>
        /// Builds a <see cref="Form"/> instance according to the current configuration.
        /// </summary>
        /// <returns></returns>
        public Form Build()
        {
            Form form = new()
            {
                Meta = _meta,
                Items = _fields,
            };

            return form;
        }
    }
}
