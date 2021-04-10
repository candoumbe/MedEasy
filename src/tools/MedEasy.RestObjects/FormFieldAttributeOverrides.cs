namespace MedEasy.RestObjects
{
    /// <summary>
    /// Attributes of a <see cref="FormField"/>
    /// </summary>
    public class FormFieldAttributeOverrides
    {
        private string _description;
        private int _maxLength;
        private FormFieldType _type;

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
        public FormFieldType Type
        {
            get => _type;
            set
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
        /// Indicate if <see cref="MaxLength"/> was explicitely set
        /// </summary>
        internal bool IsMaxLengthSet { get; private set; }
    }
}
