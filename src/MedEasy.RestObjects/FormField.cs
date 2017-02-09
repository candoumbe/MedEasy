namespace MedEasy.RestObjects
{
    /// <summary>
    /// Form field representation
    /// </summary>
    /// <remarks>
    ///     Inspired by ION spec (see http://ionwg.org/draft-ion.html#form-fields for more details)
    /// </remarks>
    public class FormField
    {
        /// <summary>
        /// Is the form enabled by 
        /// </summary>
        public bool? Enabled { get; set; }

        public bool? Secret { get; set; }


    }
}