using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly HashSet<Type> _dateTypes = new HashSet<Type>
        {
            typeof(DateTime), typeof(DateTime?),
            typeof(DateTimeOffset), typeof(DateTimeOffset?),
        };

        private static readonly HashSet<Type> _numericTypes = new HashSet<Type>
        {
            typeof(int), typeof(int?),
            typeof(float), typeof(float?),
            typeof(long), typeof(long?),
            typeof(double), typeof(double?),
            typeof(short), typeof(short?),
            typeof(decimal), typeof(decimal?),

        };

        private readonly IList<FormField> _fields;
        private string _href;
        private string _relation;
        private string _method;

        public FormBuilder()
        {
            _fields = new List<FormField>();
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
        public FormBuilder<T> AddField<TProperty>(Expression<Func<T, TProperty>> property, FormFieldAttributes attributes = null)
        {
            if(property.Body is MemberExpression me)
            {
                FormField field = new FormField { Name = me.Member.Name };
                Option<FormFieldAttributes> optionalAttributesOverride = attributes.SomeNotNull();
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

                });

                
                field.Label = field.Name;
                field.Enabled = attributes?.Enabled;
                

                _fields.Add(field);
            }

            return this;
        }

        /// <summary>
        /// Sets the link where data the form contains should be sent
        /// </summary>
        /// <param name="href">The new link</param>
        /// <returns></returns>
        public FormBuilder<T> SetHref(string href)
        {
            _href = href;

            return this;
        }


        /// <summary>
        /// Defines the relation of the form
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        public FormBuilder<T> SetRelation(string relation)
        {
            _relation = relation;

            return this;
        }

        /// <summary>
        /// Defines the method to use when sending a <see cref="Form"/>
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public FormBuilder<T> SetMethod(string method)
        {
            _method = method;


            return this;
        }

        /// <summary>
        /// Builds a <see cref="Form"/> instance according to the current configuration.
        /// </summary>
        /// <returns></returns>
        public Form Build()
        {
            Form form = new Form
            {
                Items = _fields,
            };
            if (!string.IsNullOrWhiteSpace(_relation) || !string.IsNullOrWhiteSpace(_method) || !string.IsNullOrWhiteSpace(_href))
            {
                form.Meta = new Link
                {
                    Href = _href,
                    Relation = _relation,
                    Method = _method
                };
            }
            return form;
        }
    }
}
