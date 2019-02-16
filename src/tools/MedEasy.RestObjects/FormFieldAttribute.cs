using System;
using System.Collections.Generic;
using System.Linq;

namespace MedEasy.RestObjects
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public sealed class FormFieldAttribute : Attribute
    {
        private bool _enabled;

        /// <summary>
        /// Gets/Sets  relation(s) that a <see cref="Form"/> should have for the field to appear onto it.
        /// </summary>
        public IEnumerable<string> Relations { get; set; }

        /// <summary>
        /// Name of the field
        /// </summary>
        public FormFieldType Type
        {
            get => _type; set
            {
                _type = value;
                IsTypeSet = true;
            }
        }
        /// <summary>
        /// Indicates if <see cref="Type"/> was set
        /// </summary>
        internal bool IsTypeSet { get; private set; }

        /// <summary>
        /// Indicates if the field should be accessible to the user
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                _isEnabledSet = true;
            }
        }

        private bool _isEnabledSet;

        /// <summary>
        /// Indicates if the field must be present when "submitting" the <see cref="Form"/>.
        /// </summary>
        public bool Required
        {
            get => _required;
            set
            {
                _required = value;
                IsRequiredSet = true;
            }
        }
        /// <summary>
        /// Indicates if the <see cref="Required"/> property was explicitly set
        /// </summary>
        internal bool IsRequiredSet { get; private set; }

        private bool _required;

        /// <summary>
        /// Pattern the value of the <see cref="FormField"/> should match to be valid.
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Description of the field
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                IsDescriptionSet = true;
            }
        }
        internal bool IsDescriptionSet { get; private set; }

        /// <summary>
        /// Indicates if the field's value should be accessible to the user.
        /// </summary>
        public bool Secret
        {
            get => _secret;
            set
            {
                _secret = value;
                IsSecretSet = true;
            }
        }
        private bool _secret;

        internal bool IsSecretSet { get; private set; }

        /// <summary>
        /// Minimum value for the field
        /// </summary>
        public int Min
        {
            get => _min;
            set
            {
                _min = value;
                IsMinSet = true;
            }
        }
        private int _min;
        private FormFieldType _type;
        private string _description;

        internal bool IsMinSet { get; private set; }

        private int _maxLength;

        /// <summary>
        /// Maximum length of the string
        /// </summary>
        /// <remarks>This property is only relevant for string or array</remarks>
        public int MaxLength
        {
            get => _maxLength;
            set
            {
                _maxLength = value;
                IsMaxLengthSet = true;
            }
        }
        /// <summary>
        /// Indicates if <see cref="MaxLength"/> value was explicitely set with the attribute
        /// </summary>
        internal bool IsMaxLengthSet { get; private set; }

        /// <summary>
        /// Builds a new <see cref="FormFieldAttribute"/>
        /// </summary>
        public FormFieldAttribute()
        {
            Relations = Enumerable.Empty<string>();
        }
    }
}
