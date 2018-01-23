using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.RestObjects
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public sealed class FormFieldAttribute : Attribute
    {
        /// <summary>
        /// Gets/Sets  relation(s) that a <see cref="Form"/> should have for the field to appear onto it.
        /// </summary>
        public IEnumerable<string> Relations { get; set; }

        /// <summary>
        /// Name of the field
        /// </summary>
        public FormFieldType Type { get; set; }

        /// <summary>
        /// Indicates if the field should be accessible to the user
        /// </summary>
        public bool? Enabled { get; set; }

        /// <summary>
        /// Indicates if the field must be present when submitting the <see cref="Form"/>.
        /// </summary>
        public bool? Mandatory { get; set; }

        /// <summary>
        /// Pattern the value of the <see cref="FormField"/> should match to be valid.
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Builds a new <see cref="FormFieldAttribute"/>
        /// </summary>
        public FormFieldAttribute()
        {
            Type = FormFieldType.String;
            Relations = Enumerable.Empty<string>();
        }

    }
}
