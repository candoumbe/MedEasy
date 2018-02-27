using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.RestObjects
{
    /// <summary>
    /// Attributes of a <see cref="FormField"/>
    /// </summary>
    public class FormFieldAttributes
    {
        private string _description;

        /// <summary>
        /// Description of the <see cref="FormField"/>
        /// </summary
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                IsDescriptionSet = true;
            }
        }

        /// <summary>
        /// Indicates if the description is set for the attribute
        /// </summary>
        internal bool IsDescriptionSet { get; private set; }

        /// <summary>
        /// Indicates if the <see cref="FormField"/> should be disabled
        /// </summary>
        public bool? Enabled { get; set; }

        /// <summary>
        /// Indicates if the value should be displayed as a password (<c>true<>) or not(<c>false</c>).
        /// </summary>
        public bool? Secret { get; set; }

        /// <summary>
        /// Label associated with the <see cref="FormField"/>
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Type of the value the field holds
        /// </summary>
        public FormFieldType Type { get; set; }

        /// <summary>
        /// Minimum value of the field
        /// </summary>
        public int? Min { get; set; }


        /// <summary>
        /// Maximum value of the field
        /// </summary>
        public int? Max { get; set; }

        /// <summary>
        /// Pattern the value of the field must match to be valid
        /// </summary>
        public string Pattern { get; set; }

    }
}
