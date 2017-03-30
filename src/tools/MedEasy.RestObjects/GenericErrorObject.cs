namespace MedEasy.RestObjects
{

    public class GenericErrorObject 
    {
        /// <summary>
        /// Gets/Sets the message that's is suitable to show to the end user if necessary
        /// </summary>
        public string UserMsg { get; set; }

        /// <summary>
        /// Gets/Sets the message to show in debug mode or to developers
        /// </summary>
        public string DetailedMessage { get; set; }

        /// <summary>
        /// Defines the url where to get more informations on the error that occured
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Gets/sets an additional error code number that can be used to track the error
        /// </summary>
        public string ErrorCode { get; set; }



    }
}